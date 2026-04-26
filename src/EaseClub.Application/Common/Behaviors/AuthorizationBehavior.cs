using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Clubs;
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
        private readonly IClubAdminUserRepository _clubAdminUserRepository;
        public AuthorizationBehavior(IClubAuthorizationService clubAuthorizationService,
                                     ICurrentUserService currentUserService,
                                     IClubAdminUserRepository clubAdminUserRepository,
                                     ILogger<AuthorizationBehavior<TRequest,TResponse>>logger)
        {
            _currentUserService = currentUserService;
            _clubAuthorizationService = clubAuthorizationService;
            _clubAdminUserRepository = clubAdminUserRepository;
            _logger = logger;

        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Guid.TryParse(_currentUserService.GetId(),out var userId);
            
            if (userId == Guid.Empty)
            {
                return (dynamic)Error.Unauthorized();
            }

            var isSuperAdmin = _currentUserService.GetRoles().Contains("SuperAdmin");

            // 2. Validate Persona (WHO is acting)
            if (!isSuperAdmin)
            {
                if (request is IRequireClubAdmin adminReq)
                {
                    if (!await _clubAuthorizationService.IsUserAdminOfClubAsync(userId, adminReq.ClubId))
                        return (dynamic)Error.Forbidden("You aren't an admin of this club.");
                }

                else if (request is IRequireMembership memberReq)
                {
                    if (!await _clubAuthorizationService.IsUserMemberOfClubAsync(userId, memberReq.ClubId))
                        return (dynamic)Error.Forbidden("You aren't a member of this club.");
                }



                // 3. Validate Ownership (WHAT is being touched)
                if (request is IRequireClubOwnershipValidation ownershipRequest)
                {
                    Guid? targetClubId = (await _clubAdminUserRepository.GetByIdAsync(userId))?.ClubId;

                    if (targetClubId == null)
                    {
                        return (dynamic)Error.Unauthorized();
                    }

                    foreach (var rule in ownershipRequest.Rules())
                    {
                        // We pass the targetClubId found above into the Rule's Check function
                        var isCorrectOwner = await rule.Check(_clubAuthorizationService, targetClubId.Value);

                        if (!isCorrectOwner)
                        {
                            _logger.LogWarning("Ownership Failure: Resource {ResId} does not belong to Club {ClubId}",
                                rule.ResourceId, targetClubId);
                            return (dynamic)Error.Forbidden($"The {rule.ResourceName} does not belong to the selected club.");
                        }
                    }
                }

                // 4. Validate General Resource Access (Doesn't require club ownership by user)
                if (request is IRequireResourceValidation resourceValidationRequest)
                {
                    foreach (var rule in resourceValidationRequest.Rules())
                    {
                        // For general resource validation, we pass Guid.Empty as the rule is expected to handle its own context or check current user
                        var isValid = await rule.Check(_clubAuthorizationService, Guid.Empty);

                        if (!isValid)
                        {
                            _logger.LogWarning("Resource Validation Failure: Resource {ResId} of type {ResName} failed validation",
                                rule.ResourceId, rule.ResourceName);
                            return (dynamic)Error.Forbidden($"The {rule.ResourceName} validation failed.");
                        }
                    }
                }
            }

            return await next();
        }

    }

}
