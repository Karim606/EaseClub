using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Memberships.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin
{
    public record GetMembershipsForAdminQuery(Guid ClubId): IRequest<UnifiedPaginatedResponse<MembershipsAdminDto>>,IRequireClubAdmin;

    public record MembershipsAdminDto(
        Guid Id,
        string MemberName,
        string MembershipNumber,
        string MembershipTypeName,
        string MembershipPlanName,
        DateTime CreatedDate,
        MembershipPeriod MembershipPeriod,
        MembershipStatus Status
    );
}
