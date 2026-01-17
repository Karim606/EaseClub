using EaseClub.Application.Common.interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Application.Features.Auth.Dtos;
using EaseClub.Domain.Member;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;

namespace EaseClub.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler( IMemberUserRepository memberUserRepository,IAuthSessionService authSessionService,
                  ICurrentRequestContext currentRequestContext, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<AuthTokensDto>>

    {
        public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await authSessionService.LoginAsync(request.Email, request.Password,currentRequestContext.IpAddress,currentRequestContext.DeviceInfo);
            if (result.IsSuccess)
            {
                return result;
            }
            else
            {
                logger.LogWarning("Failed login attempt for user {Email}. Reason: {Reason}", request.Email, result.TopError.ToLogObject());
                return Error.Unauthorized(description: "Invalid email Address or password.");
            }
        }
    }
}
