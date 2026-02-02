using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.Interfaces;

using MediatR;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Common.Behaviors
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull,IAuthorizeRequest
        where TResponse : IResult
    {
        private readonly IClubAdminUserRepository _clubAdminUserRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;
        public AuthorizationBehavior(IClubAdminUserRepository clubAdminUserRepository,
                                     ICurrentUserService currentUserService,
                                     ILogger<AuthorizationBehavior<TRequest,TResponse>>logger)
        {
            _clubAdminUserRepository = clubAdminUserRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {

            if (request is IRequireClubAdmin clubAdminRequest)
            {
                var userId = _currentUserService.GetId();
                Guid.TryParse(userId, out Guid userGuid);
                var clubAdmin = await _clubAdminUserRepository.GetByIdAsync(userGuid);
                Error? errorResult=null;

                if(clubAdmin == null)
                {
                    _logger.LogWarning("User with Id {UserId} is not a Club Admin and attempted to access Club Admin data",
                        userGuid);
                    errorResult = Error.Unauthorized(description:"User is not a Club Admin");
                }

                if(clubAdminRequest.ClubId != Guid.Empty && clubAdmin?.ClubId != clubAdminRequest.ClubId)
                {
                    _logger.LogWarning("User with Id {UserId} attempted to access Club data for ClubId {ClubId}" +
                        "but is only authorized for ClubId {AuthorizedClubId}",
                        userGuid, clubAdminRequest.ClubId, clubAdmin?.ClubId);
                    errorResult = Error.Forbidden(description: "User is not authorized to access this Club's data");
                }
                
                if(errorResult.HasValue)
                {
                    return (dynamic)errorResult.Value;
                }
            }

            return await next();
        }
    }

}
