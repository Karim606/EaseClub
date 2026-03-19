using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplications
{
    public class GetApplicationsQuery : IRequest<Result<UnifiedPaginatedResponse<MembershipAppDto>>>
    {
        public Guid? ClubId { get; init; }
        public ApplicationStatus? Status { get; init; }

        public PaginationRequest PaginationRequest { get; init; }
    }
}
