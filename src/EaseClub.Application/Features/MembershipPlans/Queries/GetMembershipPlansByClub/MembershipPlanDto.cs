using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub
{
    public sealed class MembershipPlanDto
    {
        public MembershipPlanDto(Guid id,string name,int durationInDay,decimal price,string? description) {
            Id = id;
            Name = name;
            DurationInDays = durationInDay;
            Description = description;
            Price = price;
        }
        public Guid Id {  get; init; }
        public string Name { get; init; }
        public int DurationInDays {  get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }

    }
}
