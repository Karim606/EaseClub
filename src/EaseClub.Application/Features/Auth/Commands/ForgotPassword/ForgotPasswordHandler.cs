using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.ForgotPassword
{
    public sealed class ForgotPasswordHandler(ILogger<ForgotPasswordCommand> logger, IAuthIdentityService identityService)
    : IRequestHandler<ForgotPasswordCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var result = await identityService.RequestResetPasswordAsync(request.Email);
            if (result.IsError)
            {
                logger.LogWarning("request to reset password for email: {email} has been failed error:{error}", request.Email, result.TopError);
                if(result.TopError.Type == ErrorKind.Failure)
                {
                    return Error.Failure(description:"failed to send mail to reset your password.");
                }
            }

            return Result.Success;
        }
    }
}
