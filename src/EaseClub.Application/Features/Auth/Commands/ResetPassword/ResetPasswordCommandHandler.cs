using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger, IAuthIdentityService identityService)
       : IRequestHandler<ResetPasswordCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var result = await identityService.ResetPasswordAsync(request.model.Email, request.model.Token, request.model.NewPassword);

            if (result.IsSuccess)
            {
                logger.LogInformation("User with email {Email} reset their password successfully.", request.model.Email);
            }
            else
            {
                logger.LogWarning("User with email {Email} failed to reset their password. Reason: {Reason}",
                    request.model.Email, result.TopError);
            }
            return result;
        }
    }
}
