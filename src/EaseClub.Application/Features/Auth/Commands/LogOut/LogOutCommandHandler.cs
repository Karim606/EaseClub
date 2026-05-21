using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.LogOut
{
    public class LogOutCommandHandler(IAuthSessionService authSessionService,ILogger<LogOutCommandHandler>logger) : IRequestHandler<LogOutCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
           var result = await authSessionService.LogoutAsync(request.RefreshToken);

            if (result.IsSuccess)
            {
                return Result.Success;
            }

            logger.LogWarning("Logout failed. Reason: {Error}", result.TopError.ToLogObject());
            return result;
        }
    }
}
