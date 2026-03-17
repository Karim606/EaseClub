using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipTypes;
using EaseClub.Application.Features.MembershipTypes.Queries;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class MembershipTypesQueryServices : BaseQueryService<MembershipType>, IMembershipTypesQueryService
    {
        public MembershipTypesQueryServices(AppDbContext context, ILogger<MembershipTypesQueryServices> logger) : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipTypeDto>>> GetMembershipTypesForUserAsync(Guid? clubId, Guid? branchId, bool? allBranchesPermitted, bool? isActive, PaginationRequest paginationParameters, CancellationToken cancellationToken)
        {
            var query = Query();

            if (clubId != null)
            {
                query = query.Where(p => p.ClubId == clubId);
            }
            if (branchId != null)
            {
                query = query.Where(p => p.PermittedBranches.Any(b => b.BranchId == branchId));
            }
            if (allBranchesPermitted != null)
            {
                query = query.Where(p => p.AllBranchesPermitted == allBranchesPermitted);
            }
            if (isActive != null)
            {
                query = query.Where(p => p.IsActive == isActive);
            }

            var list = await GetUnifiedPaginatedAsync<MembershipTypeDto,string>(query,
                paginationParameters,
                selector:p=>new MembershipTypeDto(p.Id,p.Name,p.Description,p.AllBranchesPermitted),
                orderSelector:p=>p.Name,
                cancellationToken);

            return list;
            
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipTypeAdminDto>>> GetMembershipTypesForAdminAsync(Guid? clubId, Guid? branchId, bool? allBranchesPermitted, bool? isActive, PaginationRequest paginationParameters, CancellationToken cancellationToken)
        {
            var query = Query();

            if (clubId != null)
            {
                query = query.Where(p => p.ClubId == clubId);
            }
            if (branchId != null)
            {
                query = query.Where(p => p.PermittedBranches.Any(b => b.BranchId == branchId));
            }
            if (allBranchesPermitted != null)
            {
                query = query.Where(p => p.AllBranchesPermitted == allBranchesPermitted);
            }
            if (isActive != null)
            {
                query = query.Where(p => p.IsActive == isActive);
            }

            var list = await GetUnifiedPaginatedAsync<MembershipTypeAdminDto, string>(query,
                paginationParameters,
                selector: p => new MembershipTypeAdminDto(p.Id, p.Name, p.Description, p.AllBranchesPermitted,p.IsActive,p.CreatedAt),
                orderSelector: p => p.Name,
                cancellationToken);

            return list;
        }

    }
}
