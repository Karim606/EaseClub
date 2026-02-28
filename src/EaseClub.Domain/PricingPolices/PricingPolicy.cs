using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
    public class PricingPolicy : AuditableEntity, IPricingPolicy
    {
        public Guid ClubId { get; private set; }
        public string Name { get; private set; } = default!;
        public int Priority { get; private set; }
        public bool IsIncrease { get; private set; }
        public decimal? FixedAmount { get; private set; }
        public decimal? PercentageValue { get; private set; }
        public string? MultiplierSourceKey { get; private set; }

        // Backing field for encapsulation
        private readonly List<ConditionExpression> _Conditions = new();
        public IReadOnlyList<ConditionExpression> Conditions => _Conditions.AsReadOnly();

        private PricingPolicy() { } // For EF Core

        private PricingPolicy (
            Guid id,
            Guid clubId,
            string name,
            int priority,
            bool isIncrease,
            decimal? fixedAmount,
            decimal? percentageValue,
            string? multiplierKey):base(id)
        {
            ClubId = clubId;
            Name = name;
            Priority = priority;
            IsIncrease = isIncrease;
            FixedAmount = fixedAmount;
            PercentageValue = percentageValue;
            MultiplierSourceKey = multiplierKey;
            
        }
        // Static Factory Method with Domain Validation
        public static Result<PricingPolicy> Create(
            Guid id,
            Guid clubId,
            string name,
            int priority,
            bool isIncrease,
            decimal? fixedAmount,
            decimal? percentageValue,
            string? multiplierKey,
            List<ConditionExpression>? conditions = null)
        {
            // Business Validation
            if (string.IsNullOrWhiteSpace(name))
                return PricingPolicyErrors.NameRequired;

            if (fixedAmount.HasValue && percentageValue.HasValue)
                return PricingPolicyErrors.CantCombinePercentageAndFixedAmount;

            if (!fixedAmount.HasValue && !percentageValue.HasValue)
               return PricingPolicyErrors.PercentageAndFixedAmountCantBothBeNull;

            var policy = new PricingPolicy(
                id,
                clubId,
                name,
                priority,
                isIncrease,
                fixedAmount,
                percentageValue,
                multiplierKey
                );

            if (conditions != null) policy._Conditions.AddRange(conditions);

            return policy;
        }

        public PricingPolicySnapshot ToSnapshot() =>
            new(Id, Name, Priority, IsIncrease, FixedAmount, PercentageValue, MultiplierSourceKey, _Conditions.ToList());

        public Result<Success>AddCondition(ConditionExpression condition)
        {
            _Conditions.Add(condition);
            return Result.Success;
        }

        public Result<Success> RemoveCondition(ConditionExpression condition)
        {
            _Conditions.Remove(condition);
            return Result.Success;
        }
    }
}
