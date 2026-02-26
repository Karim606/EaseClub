using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates
{
    public class ApplicationFieldDefinition : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationFieldDefinition() { }

        // 2. Private Constructor: Strictly internal to this class
        private ApplicationFieldDefinition(
            Guid id,
            Guid sectionId,
            string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership) : base(id)
        {
            SectionId = sectionId;
            Key = key;
            Type = type;
            ValidationRules = validationRules;
            VisibilityCondition = visibilityCondition;
            PersistToMembership = persistToMembership;
        }

        public Guid SectionId { get; private set; }
        public ApplicationSectionDefinition Section { get; private set;}
        public Guid? ParentFieldId { get; private set; } // For extending system fields
        public string Key { get; private set; }          // Unique identifier
        public FieldType Type { get; private set; }      // Text, Number, etc.
        public ValidationRuleSet ValidationRules { get; private set; }
        public ConditionExpression? VisibilityCondition { get; private set; }

        public int Order { get; internal set; } // Allow the Section to re-order fields
        public bool PersistToMembership { get; private set; }

        private readonly List<PricingPolicy> _PricingPolicies = new();
        public IReadOnlyList<PricingPolicy> PricingPolices => _PricingPolicies.AsReadOnly();

        // 3. Internal Factory: Only ApplicationSectionDefinition can call this
        internal static Result<ApplicationFieldDefinition> Create(
            Guid id,
            Guid sectionId,
            string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership,
            int order)
        {
            if (sectionId == Guid.Empty) return ApplicationFieldErrors.SectionIdRequired;
            if (string.IsNullOrWhiteSpace(key)) return ApplicationFieldErrors.KeyRequired;
            if (validationRules == null) return ApplicationFieldErrors.ValidationRulesRequired;

            var field = new ApplicationFieldDefinition(
                id,
                sectionId,
                key,
                type,
                validationRules,
                visibilityCondition,
                persistToMembership
            );

            field.Order = order;

            return field;
        }

        public Result<Success> Update(string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership
            )
        {
            if (string.IsNullOrWhiteSpace(key)) return ApplicationFieldErrors.KeyRequired;
            if (validationRules == null) return ApplicationFieldErrors.ValidationRulesRequired;

            Type = type;
            Key = key;
            ValidationRules = validationRules;
            VisibilityCondition = visibilityCondition;
            PersistToMembership = persistToMembership;

            return Result.Success;
        }

        internal void UpdateOrder(int order)
        {
            Order = order;
        }

        public List<Error> Validate(string? value)
        {
            return ValidationStrategyRegistry.ApplyAll(value, ValidationRules, Type);
        }

        public bool Visible(string? value)
        {
            if (VisibilityCondition == null) return true;

            return ComparisonStrategyRegistry.Evaluate(
                VisibilityCondition.Operator,
                value,
                VisibilityCondition.ExpectedValue);
        }

        public Result<Success> AttachPricingPolicy(PricingPolicy policy)
        {
            if (_PricingPolicies.Any(p => p.Id == policy.Id))
            {
                return ApplicationFieldErrors.DuplicatedPricingPolicy;
            }

            _PricingPolicies.Add(policy);

            return Result.Success;
            
        }

        public Result<Success> DeattachPricingPolicy(Guid id)
        {
            var policy = _PricingPolicies.FirstOrDefault(p => p.Id == id);

            if (policy == null)
            {
                return ApplicationFieldErrors.PricingPolicyNotAttached;
            }

            _PricingPolicies.Remove(policy);

            return Result.Success;

        }
    }
}
