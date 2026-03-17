using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipPlans.Queries;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForAdmin;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;

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
    public class MembershipPlanQueryService
    : BaseQueryService<MembershipPlan>, IMembershipPlanQueryService
    {
        public MembershipPlanQueryService(
            AppDbContext context,
            ILogger<MembershipPlanQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipPlanDto>>> GetMembershipPlansForMemberAsync(
            Guid? clubId,
            Guid? membershipTypeId,
            PaginationRequest parameters,
            CancellationToken ct) 
        {
            // 1. Build the base filter
            var query = Query().Where(p => p.ClubId == clubId);

            if(membershipTypeId.HasValue) query = query.Where(p => p.MembershipTypeId == membershipTypeId.Value);

            query = query.Where(p => p.IsActive == true);
            // 2. Optional: Add search if your parameters include it
            // query = query.ApplySearch(parameters.Search, p => p.Name);

            return await GetUnifiedPaginatedAsync<MembershipPlanDto, string>(
                query,
                parameters,
                selector: p => new MembershipPlanDto(
                    p.Id,
                    p.Name,
                    p.MaxPaymentPeriodInDays,
                    p.SubscriptionValidityInYears,
                    p.MaxFamilyMembers,
                    p.TotalPrice,
                    p.Description),
                orderSelector: p => p.Name, // Default sorting by Name
                cancellationToken: ct
            );
        }



        public async Task<Result<UnifiedPaginatedResponse<MembershipPlanAdminDto>>> GetMembershipPlansForAdminAsync(Guid? clubId, Guid? membershipTypeId, bool? isActive, PaginationRequest parameters, CancellationToken ct)
        {
            var query = Query().Where(p => p.ClubId == clubId);

            if (membershipTypeId.HasValue) query = query.Where(p => p.MembershipTypeId == membershipTypeId.Value);
            if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);


            return await GetUnifiedPaginatedAsync<MembershipPlanAdminDto, string>(
                query,
                parameters,
                selector: p => new MembershipPlanAdminDto {
                    Id = p.Id,
                    Name = p.Name,
                    MaxPaymentPeriod = p.MaxPaymentPeriodInDays,
                    SubscriptionValidityInYears = p.SubscriptionValidityInYears,
                    MaxFamilyMembers = p.MaxFamilyMembers,
                    Price = p.TotalPrice,
                    IsActive = p.IsActive,
                    MembershipTypeName = p.MembershipType.Name,
                } ,
                orderSelector: p => p.Name, // Default sorting by Name
                cancellationToken: ct
            );

        }


    }
}
