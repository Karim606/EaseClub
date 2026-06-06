using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForAdmin
{
    public record GetMembershipPlansForAdminQuery(
     Guid ClubId,
     Guid? MembershipTypeId,
     bool? IsActive,
     PaginationRequest Pagination
 ) : IRequest<Result<UnifiedPaginatedResponse<MembershipPlanAdminDto>>>,IRequireClubAdmin;

    public class MembershipPlanAdminDto
    {
        // Basic Info
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string MembershipTypeName { get; set; }

        // Pricing & Duration
        public decimal Price { get; set; }
        public int MaxPaymentPeriodInDays { get; set; }
        public int SubscriptionValidityInYears { get; set; }
        public int MaxFamilyMembers { get; set; }

        public bool InstallmentsAllowdInRenewal { get; set; }
        public decimal RenewPrice { get; set; } 
        public PaymentMode PaymentMode { get; set; }
        public EnrollmentMode EnrollmentMode { get; set; }
        // Status & Metadata
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        //public string CreatedBy { get; set; } // maybe user email or name
        //public DateTime? UpdatedAt { get; set; }
        //public string UpdatedBy { get; set; }


    }
}
