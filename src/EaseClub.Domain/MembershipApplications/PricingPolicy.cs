using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{
    public class PricingPolicy:AuditableEntity
    {
        private PricingPolicy() { }

        private PricingPolicy(
            Guid id,
            PricingTrigger trigger,
            ConditionExpression condition,
            string name,
            Money effectAmount,
            bool isPercentage,
            bool isIncrease)
            : base(id)
        {
            Trigger = trigger;
            Condition = condition;
            Name = name;
            EffectAmount = effectAmount;
            IsPercentage = isPercentage;
            IsIncrease = isIncrease;
        }

        public PricingTrigger Trigger { get; private set; }
        public ConditionExpression Condition { get; private set; }
        public string Name { get; private set; }
        public Money EffectAmount { get; private set; }
        public bool IsPercentage { get; private set; }
        public bool IsIncrease { get; private set; }

        // =========================
        // Factory
        // =========================

        public static Result<PricingPolicy> Create(
            Guid id,
            PricingTrigger trigger,
            ConditionExpression condition,
            string name,
            Money effectAmount,
            bool isPercentage,
            bool isIncrease)
        {
            if (condition == null)
                return PricingPolicyErrors.ConditionRequired;

            if (string.IsNullOrWhiteSpace(name))
                return PricingPolicyErrors.NameRequired;

            if (effectAmount == null || effectAmount.Amount <= 0)
                return PricingPolicyErrors.InvalidEffectAmount;

            return new PricingPolicy(
                id,
                trigger,
                condition,
                name,
                effectAmount,
                isPercentage,
                isIncrease);
        }

        // =========================
        // Business Logic
        // =========================

        public decimal Apply(decimal currentPrice)
        {
            decimal change;

            if (IsPercentage)
                change = currentPrice * (EffectAmount.Amount / 100m);
            else
                change = EffectAmount.Amount;

            return IsIncrease
                ? currentPrice + change
                : currentPrice - change;
        }
    }
}
