using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.ValueObjects
{
    public record ApplicationTemplateSnapshot
    {
        // The parameter names "policies" and "steps" now match 
        // the public properties "Policies" and "Steps"
        private ApplicationTemplateSnapshot() { }
        [JsonConstructor]
        public ApplicationTemplateSnapshot(
            Guid id,
            string name,
            decimal baseFee,
            List<PricingPolicySnapshot> policies,
            List<StepSnapshot> steps)
        {
            Id = id;
            Name = name;
            BaseFee = baseFee;
            Policies = policies ?? new();
            Steps = steps ?? new();
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public decimal BaseFee { get; private set; }
        public List<PricingPolicySnapshot> Policies { get; init; } = new();
        public List<StepSnapshot> Steps { get; init; } = new();
    }

    public record StepSnapshot
    {
        [JsonConstructor]
        public StepSnapshot(Guid id, string category, string title, int order, List<SectionSnapshot> sections)
        {
            Id = id;
            Category = category;
            Title = title;
            Order = order;
            Sections = sections;
        }
        private StepSnapshot() { }

        public Guid Id { get; private set; }
        public string Category { get; private set; }
        public string Title { get; private set; }
        public int Order { get; private set; }

        public List<SectionSnapshot> Sections { get; init; } = new();
    }

    public record SectionSnapshot
    {
        [JsonConstructor]
        public SectionSnapshot(Guid id, string title, int order, RepeatRule? repeatRule, List<FieldSnapshot> fields)
        {
            Id = id; // Fix: Ensure parameter names match properties
            Title = title;
            Order = order;
            RepeatRule = repeatRule;
            Fields = fields;
        }
        private SectionSnapshot() { }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public int Order { get; private set; }
        public RepeatRule? RepeatRule { get; private set; }
        public  List<FieldSnapshot> Fields { get; init; } = new();
    }


    public record FieldSnapshot
    {
      
        private FieldSnapshot() { }
        [JsonConstructor]
        public  FieldSnapshot(
                Guid id,
                string key, // The 'Link' to the PricingPolicy.MultiplierSourceKey
                string label,
                FieldType type,
                ValidationRuleSetSnapshot validationRules,
                ConditionExpressionSnapshot? visibilityCondition,
                int order)
        {
            Id = id;
            Key = key;
            Label = label;
            Type = type;
            ValidationRules = validationRules;
            VisibilityCondition = visibilityCondition;
            Order = order;

        }

        public Guid Id { get; private set; }
        public string Key { get; private set; }
        public string Label { get; private set; }
        public FieldType Type { get; private set; }
        public ValidationRuleSetSnapshot ValidationRules { get; init; } = default!;
        public ConditionExpressionSnapshot? VisibilityCondition { get; init; }

        public int Order { get; private set; }

        public List<Error> Validate(string? value)
        {
            return ValidationRules.ToDomain().Validate(value, Type);
        }

        public bool Visible(string? value)
        {
            if (VisibilityCondition == null) return true;

            return VisibilityCondition.ToDomain().IsSatisfiedBy(value);
        }
    }

    public record PricingPolicySnapshot : IPricingPolicy
    {
        [JsonConstructor]
        public PricingPolicySnapshot(
            Guid id, string name, int priority, bool isIncrease,
            decimal? fixedAmount, decimal? percentageValue,
            string? multiplierSourceKey, List<ConditionExpression>? conditions)
        {
            Id = id;
            Name = name;
            Priority = priority;
            IsIncrease = isIncrease;
            FixedAmount = fixedAmount;
            PercentageValue = percentageValue;
            MultiplierSourceKey = multiplierSourceKey;
            Conditions = conditions ?? new();
        }
        private PricingPolicySnapshot() { }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Priority { get; private set; }
        public bool IsIncrease { get; private set; }
        public decimal? FixedAmount { get; private set; }
        public decimal? PercentageValue { get; private set; }
        public string? MultiplierSourceKey { get; private set; }
        public IReadOnlyList<ConditionExpression> Conditions { get; init; }=new List<ConditionExpression>();

    }


    public record ValidationRuleSetSnapshot
    {
        public bool IsRequired { get; init; }
        public int? MinLength { get; init; }
        public int? MaxLength { get; init; }
        public string? Regex { get; init; }
        public decimal? MinValue { get; init; }
        public decimal? MaxValue { get; init; }
        public DateTime? MinDate { get; init; }
        public DateTime? MaxDate { get; init; }

        [JsonConstructor]
        public ValidationRuleSetSnapshot(
            bool isRequired,
            int? minLength,
            int? maxLength,
            string? regex,
            decimal? minValue,
            decimal? maxValue,
            DateTime? minDate,
            DateTime? maxDate)
        {
            IsRequired = isRequired;
            MinLength = minLength;
            MaxLength = maxLength;
            Regex = regex;
            MinValue = minValue;
            MaxValue = maxValue;
            MinDate = minDate;
            MaxDate = maxDate;
        }

        // Domain -> Snapshot
        public static ValidationRuleSetSnapshot FromDomain(ValidationRuleSet domain) =>
            new ValidationRuleSetSnapshot(
                domain.IsRequired,
                domain.MinLength,
                domain.MaxLength,
                domain.Regex,
                domain.MinValue,
                domain.MaxValue,
                domain.MinDate,
                domain.MaxDate
            );

        // Snapshot -> Domain
        public ValidationRuleSet ToDomain() =>
            ValidationRuleSet.Create(IsRequired, MinLength, MaxLength, Regex, MinValue, MaxValue,MinDate,MaxDate).Value;
    }

    public record ConditionExpressionSnapshot
    {
        public string DependsOnFieldKey { get; init; } = default!;
        public ComparisonOperator Operator { get; init; }
        public string ExpectedValue { get; init; } = default!;

        [JsonConstructor]
        public ConditionExpressionSnapshot(
            string dependsOnFieldKey,
            ComparisonOperator @operator,
            string expectedValue)
        {
            DependsOnFieldKey = dependsOnFieldKey;
            Operator = @operator;
            ExpectedValue = expectedValue;
        }

        public static ConditionExpressionSnapshot FromDomain(ConditionExpression domain) =>
            new ConditionExpressionSnapshot(domain.DependsOnFieldKey, domain.Operator, domain.ExpectedValue);

        public ConditionExpression ToDomain() =>
            ConditionExpression.Create(DependsOnFieldKey, Operator, ExpectedValue).Value;
    }

    public record RepeatRuleSnapshot
    {
        public string DependsOnFieldKey { get; init; } = default!;
        public RepeatMode Mode { get; init; }

        [JsonConstructor]
        public RepeatRuleSnapshot(string dependsOnFieldKey, RepeatMode mode)
        {
            DependsOnFieldKey = dependsOnFieldKey;
            Mode = mode;
        }

        public static RepeatRuleSnapshot FromDomain(RepeatRule domain) =>
            new RepeatRuleSnapshot(domain.DependsOnFieldKey, domain.Mode);

        public RepeatRule ToDomain() => RepeatRule.Create(DependsOnFieldKey, Mode).Value;
    }
}
