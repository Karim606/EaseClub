using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
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
            RepeatRule? repeatRule,
            SectionIntent intent) : base(id)
        {
            StepId = stepId;
            Title = title;
            Order = order;
            RepeatRule = repeatRule;
            Intent = intent;
        }

        public Guid StepId { get; private set; }
        public ApplicationStepDefinition Step {  get; private set; }
        public string Title { get; private set; } = null!;
        public int Order { get; internal set; }
        public RepeatRule? RepeatRule { get; private set; }
        public SectionIntent Intent { get; private set; }
        public bool IsRepeatable => RepeatRule is not null;

        private readonly List<ApplicationFieldDefinition> _Fields = new();
        public IReadOnlyList<ApplicationFieldDefinition> Fields => _Fields.AsReadOnly();

        // 3. Internal Factory: Only ApplicationStepDefinition can call this
        internal static Result<ApplicationSectionDefinition> Create(
            Guid id,
            Guid stepId,
            string title,
            int order,
            RepeatRule? repeatRule = null,
            SectionIntent intent = SectionIntent.General)
        {
            if (stepId == Guid.Empty) return ApplicationSectionDefinitionErrors.StepIdRequired;
            if (string.IsNullOrWhiteSpace(title)) return ApplicationSectionDefinitionErrors.TitleRequired;

            return new ApplicationSectionDefinition(id, stepId, title, order, repeatRule,intent);
        }

        //Add,Remove and Reorder fields
        internal Result<ApplicationFieldDefinition> CreateField(
        Guid id,
        Guid templateId,
        string key,
        string label,
        FieldType type,
        ValidationRuleSet rules,
        ConditionExpression? visibilityCondition,
        bool persistToMembership,
        bool isSystemField = false,
        List<string>? allowedValues = null)
        {
            var fieldResult = ApplicationFieldDefinition.Create(
                id,
                templateId,
                Id,
                key,
                label,
                type,
                rules,
                visibilityCondition,
                persistToMembership,
                _Fields.Count + 1,
                isSystemField,
                allowedValues
            );

            if (fieldResult.IsError)
                return fieldResult.TopError;

            _Fields.Add(fieldResult.Value);

            return fieldResult.Value;
        }

        internal Result<Success> RemoveField(Guid fieldId)
        {
            var existingField = _Fields.FirstOrDefault(f => f.Id == fieldId);
            if (existingField == null)
                return Error.NotFound("Section.FieldNotFound", "Field not found in this section.");

            if (existingField.IsSystemField)
                return Error.Conflict("Section.SystemFieldCannotBeRemoved", "System fields cannot be removed.");

                var removedOrder = existingField.Order;

            // 1. Remove the item
            _Fields.Remove(existingField);

            // 2. Re-index: Shift everything above the removed order down by 1
            foreach (var field in _Fields.Where(f => f.Order > removedOrder))
            {
                field.UpdateOrder(field.Order - 1);
            }

            return Result.Success;
        }

        internal Result<Success> ReorderFields(List<Guid> fieldIdsInOrder)
        {
            if (fieldIdsInOrder.Count != _Fields.Count)
                return Error.Validation("Section.InvalidReorder", "Count mismatch.");

            for (int i = 0; i < fieldIdsInOrder.Count; i++)
            {
                var field = _Fields.FirstOrDefault(f => f.Id == fieldIdsInOrder[i]);
                if (field == null) return Error.NotFound("Section.FieldNotFound");

                field.UpdateOrder(i + 1); // 1-based indexing
            }

            return Result.Success;
        }

        internal void SetIntent(SectionIntent intent) => Intent = intent;




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

        public Result<Success> SetRepeatRule(RepeatRule? rule)
        {
            RepeatRule = rule;
            return Result.Success;
        }


        //ToSnapShot

        public SectionSnapshot ToSnapshot()
        {
            return new SectionSnapshot(
                Id,
                Title,
                Order,
                RepeatRule, // Value Object (Immutable)
                Intent,
                _Fields.OrderBy(f => f.Order).Select(f => f.ToSnapshot()).ToList()
            );
        }
    }
}
