using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates
{
    public class ApplicationSectionDefinition : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationSectionDefinition() { }

        // 2. Private Constructor: Locked down to this class only
        private ApplicationSectionDefinition(
            Guid id,
            Guid stepId,
            string title,
            int order,
            RepeatRule? repeatRule) : base(id)
        {
            StepId = stepId;
            Title = title;
            Order = order;
            RepeatRule = repeatRule;
        }

        public Guid StepId { get; private set; }
        public string Title { get; private set; } = null!;
        public int Order { get; internal set; }
        public RepeatRule? RepeatRule { get; private set; }

        public bool IsRepeatable => RepeatRule is not null;

        private readonly List<ApplicationFieldDefinition> _Fields = new();
        public IReadOnlyList<ApplicationFieldDefinition> Fields => _Fields.AsReadOnly();

        // 3. Internal Factory: Only ApplicationStepDefinition can call this
        internal static Result<ApplicationSectionDefinition> Create(
            Guid id,
            Guid stepId,
            string title,
            int order,
            RepeatRule? repeatRule = null)
        {
            if (stepId == Guid.Empty) return ApplicationSectionDefinitionErrors.StepIdRequired;
            if (string.IsNullOrWhiteSpace(title)) return ApplicationSectionDefinitionErrors.TitleRequired;
            if (order < 0) return ApplicationSectionDefinitionErrors.InvalidOrder;

            return new ApplicationSectionDefinition(id, stepId, title, order, repeatRule);
        }

        // 4. Field Management: The Section acts as the Field Factory
        public Result<ApplicationFieldDefinition> AddNewField(
            string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool affectsPricing,
            bool persistToMembership,
            Guid? parentFieldId = null,
            int order = 0)
        {
            // Domain Rule: Uniqueness of 'Key' within this section
            if (_Fields.Any(f => f.Key == key))
                return ApplicationSectionDefinitionErrors.DuplicateFieldKey;

            var fieldResult = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                this.Id, 
                key,
                type,
                validationRules,
                visibilityCondition,
                affectsPricing,
                persistToMembership,
                parentFieldId,
                order
            );

            if (fieldResult.IsError) return fieldResult.TopError;

            _Fields.Add(fieldResult.Value);
            return fieldResult.Value;
        }

        public Result<Success> RemoveField(Guid fieldId)
        {
            var existingField = _Fields.FirstOrDefault(f => f.Id == fieldId);

            if (existingField == null)
                return ApplicationSectionDefinitionErrors.FieldDoesntExist;

            _Fields.Remove(existingField);
            return Result.Success;
        }

        public Result<Success> SetRepeatRule(RepeatRule? rule)
        {
            RepeatRule = rule;
            return Result.Success;
        }

        public Result<int> EvaluateRepeatRule(string actualValue)
        {
            if (!IsRepeatable) return 0;
            return RepeatRule!.Evaluate(actualValue);
        }
    }
}
