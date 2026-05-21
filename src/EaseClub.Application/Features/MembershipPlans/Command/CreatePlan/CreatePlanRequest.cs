using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.CreatePlan
{
    public class CreatePlanRequest
    {
        public Guid MembershipTypeId { get; init; }
        public string Name { get; init; }
        public decimal Price { get; init; }
        public int SubscriptionValidityInYears { get; init; }
        public int MaxFamilyMembers { get; init; }
        public int DurationInDays { get; init; }
        public EnrollmentMode EnrollmentMode { get; init; }
        public Guid? ApplicationTemplateId { get; init; } = null;
        public bool InstallmentsAllowedInRenewal { get; init; } = false;
        public decimal RenewPrice { get; init; }
        public PaymentMode paymentMode { get; init; }
        public List<Guid> InstallmentTemplateIds { get; init; } = new();
    }
}
