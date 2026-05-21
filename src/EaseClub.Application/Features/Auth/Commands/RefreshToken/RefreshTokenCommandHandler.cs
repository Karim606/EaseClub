using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler(IAuthSessionService sessionService,
        ICurrentRequestContext currentRequestContext,ILogger<RefreshTokenCommandHandler> logger)
        : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
    {
        public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = await sessionService.RefreshAsync(request.unHashedRefreshToken,currentRequestContext.IpAddress,currentRequestContext.DeviceInfo);

            if (result.IsError)
            {
                logger.LogWarning("Refresh token attempt failed from IP {IpAddress} using device {DeviceInfo}. Reason: {Reason}",
                    currentRequestContext.IpAddress,currentRequestContext.DeviceInfo,result.TopError);
            }
            return result;
        }
    }
}
