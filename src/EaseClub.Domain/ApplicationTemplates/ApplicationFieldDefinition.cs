using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
            Guid templateId,
            string key,
            string label,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership,
            bool isSystem,
            List<string>? allowedValues=null) : base(id)
        {
            SectionId = sectionId;
            TemplateId = templateId;
            Key = key;
            Label = label;
            Type = type;
            ValidationRules = validationRules;
            VisibilityCondition = visibilityCondition;
            PersistToMembership = persistToMembership;
            AllowedValues = allowedValues;
        }

        public Guid SectionId { get; private set; }
        public Guid TemplateId { get; private set; }
        public ApplicationSectionDefinition Section { get; private set;}
        //public Guid? ParentFieldId { get; private set; } // For extending system fields
        public string Key { get; private set; }          // Unique identifier
        public string Label { get; private set; }

        public FieldType Type { get; private set; }      // Text, Number, etc.
        public ValidationRuleSet ValidationRules { get; private set; }
        public ConditionExpression? VisibilityCondition { get; private set; }

        public int Order { get; internal set; } // Allow the Section to re-order fields
        public bool PersistToMembership { get; private set; }
        public bool IsSystemField { get; init; } //
        public List<string>? AllowedValues { get; private set; }


        // 3. Internal Factory: Only ApplicationSectionDefinition can call this
        internal static Result<ApplicationFieldDefinition> Create(
            Guid id,
            Guid templateId,
            Guid sectionId,
            string key,
            string label,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership,
            int order,
            bool isSystemField=false,
            List<string>? allowedValues=null)
        {
            if (sectionId == Guid.Empty) return ApplicationFieldErrors.SectionIdRequired;
            if (templateId == Guid.Empty) return ApplicationFieldErrors.TemplateIdRequired;
            if (string.IsNullOrWhiteSpace(key)) return ApplicationFieldErrors.KeyRequired;
            if (string.IsNullOrWhiteSpace(label)) return ApplicationFieldErrors.KeyRequired;
            if (type == FieldType.Enum && (allowedValues == null || !allowedValues.Any()))
                return Error.Validation("Field.AllowedValuesRequired", "Allowed values are required for Enum fields.");
            if (validationRules == null) return ApplicationFieldErrors.ValidationRulesRequired;


            var field = new ApplicationFieldDefinition(
                id,
                sectionId,
                templateId,
                key,
                label,
                type,
                validationRules,
                visibilityCondition,
                isSystemField,
                persistToMembership
            );

            field.Order = order;
            if(field.Type==FieldType.Enum)
            field.SetAllowedValues(allowedValues);
            return field;
        }

        public Result<Success> Update(string label,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership,
            List<string>? allowedValues=null
            )
        {
            if(IsSystemField) return ApplicationFieldErrors.SystemFieldCannotBeUpdated;
            if (string.IsNullOrWhiteSpace(label)) return ApplicationFieldErrors.LabelRequired;
            if (validationRules == null) return ApplicationFieldErrors.ValidationRulesRequired;
            if (Type == FieldType.Enum && (allowedValues == null || !allowedValues.Any()))
                return Error.Validation("Field.AllowedValuesRequired", "Enum fields require allowed values.");

            Label = label;
            ValidationRules = validationRules;
            VisibilityCondition = visibilityCondition;
            PersistToMembership = persistToMembership;

            if (Type == FieldType.Enum) SetAllowedValues(allowedValues);

            return Result.Success;
        }

        internal void UpdateOrder(int order)
        {
            Order = order;
        }

        private Result<Success> SetAllowedValues(List<string>? values ) 
        {

            //  Clean the data: Remove whitespace, ignore empty strings, remove duplicates
            AllowedValues = values!
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .GroupBy(v => v.ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

            return Result.Success;
        }

        //ToSnapshot
        public FieldSnapshot ToSnapshot()
        {
            return new FieldSnapshot(
                Id,
                Key,
                Label,
                Type, // Enum (Text, Number, Date, etc.)
                ValidationRules.ToSnapshot(),      // Value Object
                VisibilityCondition?.ToSnapshot(),   // Value Object
                Order,
                IsSystemField
            );
        }
    }
}
