using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesForAdmin
{
    public record GetMembershipTypesForAdminQuery(
        Guid ClubId,
        Guid? BranchId,
        bool? AccessToAllBranches,
        bool? IsActive,
        PaginationRequest Pagination
    ) : IRequest<Result<UnifiedPaginatedResponse<MembershipTypeAdminDto>>>;

    public class MembershipTypeAdminDto
    {
        
        public MembershipTypeAdminDto(Guid id, string name, string? description, bool allBranchesPermitted, bool isActive,DateTime createdAt)
        {
            IsActive = isActive;
            Id = id;
            Name = name;
            Description = description;
            AllBranchesPermitted = allBranchesPermitted;
            IsActive = isActive;
            CreatedAt = createdAt;

        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool AllBranchesPermitted { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
