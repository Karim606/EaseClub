using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Behaviors
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull,IAuthorizeRequest
        where TResponse : IResult
    {
        private readonly IClubAuthorizationService _clubAuthorizationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;
        public AuthorizationBehavior(IClubAuthorizationService clubAuthorizationService,
                                     ICurrentUserService currentUserService,
                                     ILogger<AuthorizationBehavior<TRequest,TResponse>>logger)
        {
            _currentUserService = currentUserService;
            _clubAuthorizationService = clubAuthorizationService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {

            if (request is IRequireClubAdmin clubAdminRequest)
            {
                var userId = _currentUserService.GetId();
                Guid.TryParse(userId, out Guid userGuid);

                var isUserAdminOfClub = await _clubAuthorizationService.IsUserAdminOfClubAsync(userGuid, clubAdminRequest.ClubId);
                
                if (!isUserAdminOfClub) {
                    _logger.LogWarning("User {UserId} is NOT an admin for club {ClubId}", userGuid, clubAdminRequest.ClubId);
                    return (dynamic)Error.Forbidden(description: "You aren't allowed to manage this club's resources.");
                    
                }

                
                if(request is IRequireClubOwnershipValidation ownershipRequest)
                {
                    foreach (var rule in ownershipRequest.Rules())
                    {
                        var allowed = await rule.Check(_clubAuthorizationService);
                        
                        if (!allowed)
                        {
                            _logger.LogWarning(
                            "Authorization Failed: Resource {ResourceName} with ID {ResourceId} does not belong to Club {ClubId}. User: {UserId}",
                            rule.ResourceName,
                            rule.ResourceId,
                            clubAdminRequest.ClubId,
                            userGuid);

                            return (dynamic)Error.Forbidden(
                                description: $"The {rule.ResourceName} you are trying to access does not belong to your club.");
                        }
                    }
                }
                

            }

            return await next();
        }
    }

}
