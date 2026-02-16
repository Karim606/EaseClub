using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Errors;

namespace EaseClub.Domain.MembershipApplications
{
    public class ApplicationStepInstance : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationStepInstance() { }

        // 2. Private Constructor: Only internal factory can call this
        private ApplicationStepInstance(Guid id, Guid applicationId, Guid templateStepId)
            : base(id)
        {
            ApplicationId = applicationId;
            TemplateStepId = templateStepId;
            IsCompleted = false;
            IsLocked = false;
        }

        public Guid ApplicationId { get; private set; }
        public Guid TemplateStepId { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsLocked { get; private set; }

        private readonly List<ApplicationSectionInstance> _sections = new();
        public IReadOnlyList<ApplicationSectionInstance> Sections => _sections;

        // 3. INTERNAL Factory: Only MembershipApplication can create this
        internal static Result<ApplicationStepInstance> Create(
            Guid id,
            Guid applicationId,
            Guid templateStepId)
        {
            if (applicationId == Guid.Empty)
                return ApplicationStepInstanceErrors.ApplicationIdRequired;

            if (templateStepId == Guid.Empty)
                return ApplicationStepInstanceErrors.TemplateStepIdRequired;

            return new ApplicationStepInstance(id, applicationId, templateStepId);
        }

        // =========================
        // Business Methods
        // =========================

        // 4. THE GATEKEEPER: Spawn SectionInstance through StepInstance
        public Result<ApplicationSectionInstance> AddNewSectionInstance(Guid templateSectionId, int index)
        {
            if (IsLocked)
                return ApplicationStepInstanceErrors.StepLocked;

            // Integrity Check: Prevent duplicate indices for the same section 
            // (e.g., prevents having two "Member #1" entries in the same step)
            if (_sections.Any(s => s.TemplateSectionId == templateSectionId && s.Index == index))
                return ApplicationStepInstanceErrors.DuplicateSectionInstance;

            // Call internal static Create on ApplicationSectionInstance
            var sectionResult = ApplicationSectionInstance.Create(
                Guid.NewGuid(),
                this.Id, // Link to this Step Instance
                templateSectionId,
                index
            );

            if (sectionResult.IsError) return sectionResult.TopError;

            _sections.Add(sectionResult.Value);
            return sectionResult.Value;
        }

        public Result<Success> Complete()
        {
            if (IsLocked)
                return ApplicationStepInstanceErrors.StepLocked;

            IsCompleted = true;
            return Result.Success;
        }

        public void Lock()
        {
            IsLocked = true;
        }
    }
}