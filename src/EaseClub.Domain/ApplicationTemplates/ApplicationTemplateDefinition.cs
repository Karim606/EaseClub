using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates
{
    public class ApplicationTemplateDefinition : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationTemplateDefinition() { }

        // 2. Private Constructor for strict creation
        private ApplicationTemplateDefinition(Guid id, Guid clubId, Guid? previousTemplateId, string name) : base(id)
        {
            ClubId = clubId;
            PreviousTemplateId = previousTemplateId;
            Name = name;
        }

        public Guid ClubId { get; private set; }
        public string Name { get; private set; }
        public int Version { get; private set; } = 1;
        public Guid? PreviousTemplateId { get; private set; }
        public bool IsActive { get; private set; } = true;

        private readonly List<ApplicationStepDefinition> _Steps = new();
        public IReadOnlyList<ApplicationStepDefinition> Steps => _Steps.AsReadOnly();

        // 3. Public Static Factory (Public because App Layer uses this)
        public static Result<ApplicationTemplateDefinition> Create(Guid id, Guid clubId, Guid membershipTypeId, Guid? previousTemplateId, string name)
        {
            if (clubId == Guid.Empty) return ApplicationTemplateDefinitionErrors.ClubIdRequired;
            if (membershipTypeId == Guid.Empty) return ApplicationTemplateDefinitionErrors.MembershipTypeIdRequired;
            if (previousTemplateId.HasValue && previousTemplateId == id) return ApplicationTemplateDefinitionErrors.InvalidPreviousTemplateId;
            if (string.IsNullOrEmpty(name)) return ApplicationTemplateDefinitionErrors.InvalidName;

            return new ApplicationTemplateDefinition(id, clubId, previousTemplateId, name);
        }

        // 4. THE AGGREGATE GATEKEEPER: Create Step through Template
        public Result<ApplicationStepDefinition> AddNewStep(string category, string title, int order)
        {
            if (_Steps.Any(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                return ApplicationTemplateDefinitionErrors.DuplicateStepTitle;

            
            var stepResult = ApplicationStepDefinition.Create(
                Guid.NewGuid(),
                this.Id,
                category,
                title,
                order
            );

            if (stepResult.IsError) return stepResult.TopError;

            _Steps.Add(stepResult.Value);
            return stepResult.Value;
        }

        public Result<Success> RemoveStep(Guid stepId)
        {
            var existingStep = _Steps.FirstOrDefault(s => s.Id == stepId);

            if (existingStep == null)
                return ApplicationTemplateDefinitionErrors.StepDoesntExist;

            _Steps.Remove(existingStep);
            return Result.Success;
        }
    }
}
