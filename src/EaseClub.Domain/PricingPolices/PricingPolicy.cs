using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
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
    public class PricingPolicy : AuditableEntity,IHaveClub
    {
        public Guid ClubId { get; private set; }
        public string Name { get; private set; } = default!;
        public bool IsIncrease { get; private set; }
        public decimal? FixedAmount { get; private set; }
        public decimal? PercentageValue { get; private set; }
        public string? MultiplierSourceKey { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Backing field for encapsulation
        private readonly List<ConditionExpression> _Conditions = new();
        public IReadOnlyList<ConditionExpression> Conditions => _Conditions.AsReadOnly();

        private PricingPolicy() { } // For EF Core

        private PricingPolicy (
            Guid id,
            Guid clubId,
            string name,
            bool isIncrease,
            decimal? fixedAmount,
            decimal? percentageValue,
            string? multiplierKey):base(id)
        {
            ClubId = clubId;
            Name = name;
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
                isIncrease,
                fixedAmount,
                percentageValue,
                multiplierKey
                );

            if (conditions != null) policy._Conditions.AddRange(conditions);

            return policy;
        }

        public Result<Success> Update(
        string name,
        int priority,
        bool isIncrease,
        decimal? fixedAmount,
        decimal? percentageValue,
        string? multiplierKey,
        List<ConditionExpression> conditions)
        {
            // 1. Maintain Business Invariants
            if (string.IsNullOrWhiteSpace(name))
                return PricingPolicyErrors.NameRequired;

            if (fixedAmount.HasValue && percentageValue.HasValue)
                return PricingPolicyErrors.CantCombinePercentageAndFixedAmount;

            if (!fixedAmount.HasValue && !percentageValue.HasValue)
                return PricingPolicyErrors.PercentageAndFixedAmountCantBothBeNull;

            // 2. Apply Updates
            Name = name;
            IsIncrease = isIncrease;
            FixedAmount = fixedAmount;
            PercentageValue = percentageValue;
            MultiplierSourceKey = multiplierKey;

            // 3. Reconcile Condition List
            // We clear and re-add to ensure the state perfectly matches the new requested configuration
            _Conditions.Clear();
            _Conditions.AddRange(conditions);

            return Result.Success;
        }

        public PricingPolicySnapshot ToSnapshot(int priority) =>
            new(Id, Name, priority, IsIncrease, FixedAmount, PercentageValue, MultiplierSourceKey, _Conditions.ToList());

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
