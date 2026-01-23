using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Member;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            
            var phoneExists = await memberUserRepo.PhoneExistsAsync(request.PhoneNumber);
           
            var emailExists = await memberUserRepo.EmailExistsAsync(request.Email);

            if (emailExists||phoneExists) {
                logger.LogWarning("Registration conflict: EmailExists={EmailExists}, PhoneExists={PhoneExists}", emailExists.ToString(),
                    phoneExists.ToString());
                return Error.Conflict(description:"Unable to complete registration with provided credentials");
            }

            // 1. Start Transaction
            using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2. Call Identity Service (Only adds to Change Tracker)
                var identityResult = await identityService.RegisterUserAsync(request.Email, request.Password);
                if (!identityResult.IsSuccess) return identityResult.TopError;

                var userId = identityResult.Value;

                var phoneResult = PhoneNumber.Create(request.PhoneNumber);
                if (phoneResult.IsError)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return phoneResult.TopError;
                }

                var emailResult = Email.Create(request.Email);
                if (emailResult.IsError)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return emailResult.TopError;
                }

                // 3. Call Member Repo (Only adds to Change Tracker)
                var member = MemberUser.Create(userId, request.FirstName, request.LastName,
                             phoneResult.Value, emailResult.Value);
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
