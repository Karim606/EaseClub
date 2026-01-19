using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;
using EaseClub.Domain.Common.ValueObjects;

using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler(IAuthIdentityService identityService,
            IMemberUserRepository memberUserRepo, IUnitOfWork unitOfWork,
            IAuthSessionService sessionService, ICurrentRequestContext currentRequestContext,
            ILogger<RegisterCommandHandler> logger
        ) : IRequestHandler<RegisterCommand, Result<AuthTokensDto>>
    {

        public async Task<Result<AuthTokensDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {

            // 1. Start Transaction
            using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2. Call Identity Service (Only adds to Change Tracker)
                var identityResult = await identityService.RegisterUserAsync(request.Email, request.Password);
                if (!identityResult.IsSuccess) return identityResult.TopError;

                var userId = identityResult.Value;

                // 3. Call Member Repo (Only adds to Change Tracker)
                var member = MemberUser.Create(userId, request.FirstName, request.LastName,new PhoneNumber(request.PhoneNumber),new Email(request.Email));
                await memberUserRepo.AddAsync(member);

                var tokens = await sessionService.GenerateAuthTokens(
                    userId: userId,
                    name:request.FirstName+' '+request.LastName,
                    email: request.Email,
                    ip: currentRequestContext.IpAddress,
                    deviceInfo: currentRequestContext.DeviceInfo
                );

                // 4. ATOMIC COMMIT
                // This saves BOTH the AuthUser and the MemberUser in one go
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return tokens;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger.LogError(ex, "Transaction failed for {Email}", request.Email);
                return Error.Unexpected("Registration failed.");
            }
        }
    }
}
