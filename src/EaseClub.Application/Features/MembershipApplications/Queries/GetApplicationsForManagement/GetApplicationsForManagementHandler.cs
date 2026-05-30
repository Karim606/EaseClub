using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationsForManagement
{
    public class GetApplicationsForManagementHandler(IMembershipApplicationQueryService membershipApplicationQueryService,
        ILogger<GetApplicationsForManagementHandler> logger) : IRequestHandler<GetApplicationsForManagementQuery, Result<OffsetPaginatedResult<MembershipAppAdminDto>>>
    {
        public async Task<Result<OffsetPaginatedResult<MembershipAppAdminDto>>> Handle(GetApplicationsForManagementQuery request, CancellationToken cancellationToken)
        {
            if (request.filters != null && request.filters.Status.HasValue
                && request.filters.Status == ApplicationStatus.Draft) 
                return Error.Validation(description: "Cannot filter by Draft status");

            var list = await membershipApplicationQueryService.GetMembershipApplicationsForManagementAsync(request.ClubId,request.filters,request.pagination,cancellationToken);

            return list;
        }
    }
}
