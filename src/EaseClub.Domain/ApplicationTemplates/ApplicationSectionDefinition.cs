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
using static System.Collections.Specialized.BitVector32;

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
        public ApplicationStepDefinition Step {  get; private set; }
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

            return new ApplicationSectionDefinition(id, stepId, title, order, repeatRule);
        }

        // 4. Field Management: The Section acts as the Field Factory
        public Result<ApplicationFieldDefinition> AddNewField(
            string key,
            FieldType type,
            ValidationRuleSet validationRules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership,
            int order = 0)
        {
            // Domain Rule: Uniqueness of 'Key' within this section
            if (_Fields.Any(f => f.Key == key))
                return ApplicationSectionDefinitionErrors.DuplicateFieldKey;

            if (order < 0 || order > _Fields.Count) return ApplicationSectionDefinitionErrors.InvalidFieldOrder;

            var fieldResult = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                this.Id, 
                key,
                type,
                validationRules,
                visibilityCondition,
                persistToMembership,
                order
            );

            if (fieldResult.IsError) return fieldResult.TopError;

            // 3. SHIFTING LOGIC: Move existing fields forward
            foreach (var existingField in _Fields.Where(f => f.Order >= order))
            {
                existingField.UpdateOrder(existingField.Order + 1);
            }

            _Fields.Add(fieldResult.Value);


            return fieldResult.Value;
        }

        public Result<Success> Update(string title,RepeatRule? rule=null)
        {
            if (string.IsNullOrWhiteSpace(title)) return ApplicationSectionDefinitionErrors.TitleRequired;

            Title = title;
            RepeatRule = rule;
            return Result.Success;
        }
        internal void UpdateOrder(int order)
        {
            Order = order;
        }

        public Result<Success> RemoveField(Guid fieldId)
        {
            var existingField = _Fields.FirstOrDefault(f => f.Id == fieldId);

            if (existingField == null)
                return ApplicationSectionDefinitionErrors.FieldDoesntExist;
            
            var removedOrder = existingField.Order;

            _Fields.Remove(existingField);

            // 3. SHIFTING LOGIC: Close the gap
            foreach (var remainingField in _Fields.Where(s => s.Order > removedOrder))
            {
                remainingField.UpdateOrder(remainingField.Order - 1);
            }

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
