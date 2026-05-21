using Microsoft.Extensions.Logging;
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
    public class GetApplicationsQueryHandler(IMembershipApplicationQueryService queryService,
        ICurrentUserService currentUserService,
        IClubRepository clubRepository,
        IClubAuthorizationService clubAuthorizationService,
        ILogger<GetApplicationsQueryHandler> logger)
    : IRequestHandler<GetApplicationsQuery,Result<UnifiedPaginatedResponse<MembershipAppDto>> >
    {

        public async Task<Result<UnifiedPaginatedResponse<MembershipAppDto>>> Handle(
            GetApplicationsQuery request,
            CancellationToken cancellationToken)
        {
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (res == false) { logger.LogError("Unauthorized error in GetApplicationsQueryHandler: {Error}", Error.Unauthorized().ToLogObject()); return Error.Unauthorized(); }
            return await queryService.GetMembershipApplicationSummaryAsync(request.ClubId,
                     userId, request.Status, request.PaginationRequest, cancellationToken);
        }
    }
}
