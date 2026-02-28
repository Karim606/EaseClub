using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
    public interface IPricingPolicy
    {
        Guid Id { get; }
        string Name { get; }
        int Priority { get; }
        bool IsIncrease { get; }
        decimal? FixedAmount { get; }
        string? MultiplierSourceKey { get; }
        decimal? PercentageValue { get; }
        IReadOnlyList<ConditionExpression>? Conditions { get; }
    }
}
