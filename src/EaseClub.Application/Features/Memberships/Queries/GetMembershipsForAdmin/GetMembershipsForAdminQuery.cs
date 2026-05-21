using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Memberships.ValueObjects;
using MediatR;

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin
{
    public record GetMembershipsForAdminQuery(
        Guid ClubId,
        MembershipStatus? Status,
        string Search,
        PaginationRequest PaginationRequest) : IRequest<Result<UnifiedPaginatedResponse<MembershipsAdminDto>>>, IRequireClubAdmin;

    public record MembershipsAdminDto(
        Guid Id,
        string MemberName,
        string MembershipNumber,
        string MembershipTypeName,
        string MembershipPlanName,
        bool IsFamilyMembership,
        MembershipCycle CurrentCycle,
        DateTime CreatedAt,
        MembershipStatus Status
    );
}
