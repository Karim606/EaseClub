using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplications
{
    public class GetApplicationsQueryHandler(
        IMembershipApplicationQueryService queryService,
        ICurrentUserService currentUserService,
        IClubRepository clubRepository,
        IClubAuthorizationService clubAuthorizationService
        )
    : IRequestHandler<GetApplicationsQuery,Result<UnifiedPaginatedResponse<MembershipAppDto>> >
    {

        public async Task<Result<UnifiedPaginatedResponse<MembershipAppDto>>> Handle(
            GetApplicationsQuery request,
            CancellationToken cancellationToken)
        {
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);
            if(res == false)
            {
                return Error.Unauthorized();
            }

            var roles = currentUserService.GetRoles();

            if (roles.Contains("ClubAdmin"))
            {
                if (request.ClubId != null) {
                    var adminOfClub = await clubAuthorizationService.IsUserAdminOfClubAsync(userId, request.ClubId.Value);

                    if (adminOfClub == false)
                    {
                        return Error.Forbidden();
                    }

                    if(request.UserId != null)
                    {
                        var memberOfClub = await clubAuthorizationService.IsUserMemberOfClubAsync(request.UserId.Value,request.ClubId.Value);
                    }

                    if(request.Status == ApplicationStatus.Draft) return Error.Forbidden();
                }

            }

            else if (roles.Contains("Member"))
            {
                if(userId != request.UserId) { return Error.Forbidden(); }
            }

                return await queryService.GetMembershipApplicationSummaryAsync(request.ClubId,
                     request.UserId, request.Status, request.PaginationRequest, cancellationToken);
        }
    }
}
