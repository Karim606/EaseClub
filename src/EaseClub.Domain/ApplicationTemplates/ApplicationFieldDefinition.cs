using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
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
            Guid? parentFieldId,
            string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool affectsPricing,
            bool persistToMembership) : base(id)
        {
            SectionId = sectionId;
            ParentFieldId = parentFieldId;
            Key = key;
            Type = type;
            ValidationRules = validationRules;
            VisibilityCondition = visibilityCondition;
            AffectsPricing = affectsPricing;
            PersistToMembership = persistToMembership;
        }

        public Guid SectionId { get; private set; }
        public Guid? ParentFieldId { get; private set; } // For extending system fields
        public string Key { get; private set; }          // Unique identifier
        public FieldType Type { get; private set; }      // Text, Number, etc.
        public ValidationRuleSet ValidationRules { get; private set; }
        public ConditionExpression? VisibilityCondition { get; private set; }

        public int Order { get; internal set; } // Allow the Section to re-order fields
        public bool AffectsPricing { get; private set; }
        public bool PersistToMembership { get; private set; }

        // 3. Internal Factory: Only ApplicationSectionDefinition can call this
        internal static Result<ApplicationFieldDefinition> Create(
            Guid id,
            Guid sectionId,
            string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool affectsPricing,
            bool persistToMembership,
            Guid? parentFieldId = null,
            int order = 0)
        {
            if (sectionId == Guid.Empty) return ApplicationFieldErrors.SectionIdRequired;
            if (string.IsNullOrWhiteSpace(key)) return ApplicationFieldErrors.KeyRequired;
            if (order < 0) return ApplicationFieldErrors.InvalidOrder;
            if (validationRules == null) return ApplicationFieldErrors.ValidationRulesRequired;

            var field = new ApplicationFieldDefinition(
                id,
                sectionId,
                parentFieldId,
                key,
                type,
                validationRules,
                visibilityCondition,
                affectsPricing,
                persistToMembership
            );

            field.Order = order;

            return field;
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
    }
}
