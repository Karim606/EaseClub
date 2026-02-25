using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates
{
    public class ApplicationTemplateDefinition : AuditableEntity,IHaveClub
    {
        // 1. EF Core Constructor
        private ApplicationTemplateDefinition() { }

        // 2. Private Constructor for strict creation
        private ApplicationTemplateDefinition(Guid id, Guid clubId, string name) : base(id)
        {
            ClubId = clubId;
            Name = name;
        }

        public Guid ClubId { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; } = true;

        private readonly List<ApplicationStepDefinition> _Steps = new();
        public IReadOnlyList<ApplicationStepDefinition> Steps => _Steps.AsReadOnly();

        // 3. Public Static Factory (Public because App Layer uses this)
        public static Result<ApplicationTemplateDefinition> Create(Guid id, Guid clubId, string name)
        {
            if (clubId == Guid.Empty) return ApplicationTemplateDefinitionErrors.ClubIdRequired;
            if (string.IsNullOrEmpty(name)) return ApplicationTemplateDefinitionErrors.InvalidName;

            return new ApplicationTemplateDefinition(id, clubId, name);
        }

        public Result<Success> Update(string name)
        {
            if (string.IsNullOrEmpty(name)) return ApplicationTemplateDefinitionErrors.InvalidName;
            
            Name = name;
            return Result.Success;
        }

        // 4. THE AGGREGATE GATEKEEPER: Create Step through Template
        public Result<ApplicationStepDefinition> AddNewStep(string category, string title, int order)
        {
            if (_Steps.Any(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                return ApplicationTemplateDefinitionErrors.DuplicateStepTitle;

            if (order < 0 || order > _Steps.Count) return ApplicationTemplateDefinitionErrors.InvalidStepOrder;

            var stepResult = ApplicationStepDefinition.Create(
                Guid.NewGuid(),
                this.Id,
                category,
                title,
                order
            );

            if (stepResult.IsError) return stepResult.TopError;

            // 3. SHIFTING LOGIC: Move existing steps forward
            foreach (var existingStep in _Steps.Where(s => s.Order >= order))
            {
                existingStep.UpdateOrder(existingStep.Order + 1);
            }

            _Steps.Add(stepResult.Value);

            return stepResult.Value;
        }

        public Result<Success> RemoveStep(Guid stepId)
        {
            var existingStep = _Steps.FirstOrDefault(s => s.Id == stepId);

            if (existingStep == null)
                return ApplicationTemplateDefinitionErrors.StepDoesntExist;

            int removedOrder = existingStep.Order;

            _Steps.Remove(existingStep);

            // 3. SHIFTING LOGIC: Close the gap
            foreach (var remainingStep in _Steps.Where(s => s.Order > removedOrder))
            {
                remainingStep.UpdateOrder(remainingStep.Order - 1);
            }

            return Result.Success;
        }
    }
}
