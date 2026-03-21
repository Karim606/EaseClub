using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember
{
    public sealed class MembershipPlanDto
    {
        public MembershipPlanDto(Guid id,string name,int maxPaymentPeriod,int subscriptionValidity,int maxFamilyMembers,decimal price,string? description) {
            Id = id;
            Name = name;
            MaxPaymentPeriodInDays = maxPaymentPeriod;
            Description = description;
            Price = price;
            SubscriptionValidityInYears = subscriptionValidity;
            MaxFamilyMembers = maxFamilyMembers;
        }
        public Guid Id {  get; init; }
        public string Name { get; init; }
        public int MaxPaymentPeriodInDays {  get; init; }
        public string? Description { get; init; }
        public int SubscriptionValidityInYears { get; init; }
        public int MaxFamilyMembers { get; init; }
        public decimal Price { get; init; }

    }
}
