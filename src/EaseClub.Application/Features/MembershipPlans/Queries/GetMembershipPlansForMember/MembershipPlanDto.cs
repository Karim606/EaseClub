using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember
{
    public sealed class MembershipPlanDto
    {
        public MembershipPlanDto(Guid id,string name,int maxPaymentPeriod,int subscriptionValidity,int maxFamilyMembers,decimal price,EnrollmentMode enrollmentMode,PaymentMode paymentMode,
            bool installmentsAllowedInRenewal,decimal renewPrice,string? description) {
            Id = id;
            Name = name;
            MaxPaymentPeriodInDays = maxPaymentPeriod;
            Description = description;
            Price = price;
            SubscriptionValidityInYears = subscriptionValidity;
            MaxFamilyMembers = maxFamilyMembers;
            EnrollmentMode = enrollmentMode;
            PaymentMode = paymentMode;
            InstallmentsAllowedInRenewal = installmentsAllowedInRenewal;
            RenewPrice = renewPrice;

        }
        public Guid Id {  get; init; }
        public string Name { get; init; }
        public int MaxPaymentPeriodInDays {  get; init; }
        public string? Description { get; init; }
        public int SubscriptionValidityInYears { get; init; }
        public int MaxFamilyMembers { get; init; }
        public decimal Price { get; init; }
        public EnrollmentMode EnrollmentMode { get; init; }
        public decimal RenewPrice { get; init; }
        public bool InstallmentsAllowedInRenewal { get; init; }
        public PaymentMode PaymentMode { get; init; }


    }
}
