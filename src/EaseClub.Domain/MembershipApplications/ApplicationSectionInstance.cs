using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Errors;

namespace EaseClub.Domain.MembershipApplications
{
    public class ApplicationSectionInstance : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationSectionInstance() { }

        // 2. Private Constructor: Strictly controlled
        private ApplicationSectionInstance(
            Guid id,
            Guid stepInstanceId,
            Guid templateSectionId,
            int index) : base(id)
        {
            StepInstanceId = stepInstanceId;
            TemplateSectionId = templateSectionId;
            Index = index;
        }

        public Guid StepInstanceId { get; private set; }
        public Guid TemplateSectionId { get; private set; }
        public int Index { get; private set; }

        private readonly List<ApplicationFieldValue> _Fields = new();
        public IReadOnlyList<ApplicationFieldValue> Fields => _Fields.AsReadOnly();

        // 3. INTERNAL Factory: Only ApplicationStepInstance can call this
        internal static Result<ApplicationSectionInstance> Create(
            Guid id,
            Guid stepInstanceId,
            Guid templateSectionId,
            int index)
        {
            if (stepInstanceId == Guid.Empty)
                return ApplicationSectionInstanceErrors.StepInstanceRequired;

            if (templateSectionId == Guid.Empty)
                return ApplicationSectionInstanceErrors.TemplateSectionRequired;

            if (index < 0)
                return ApplicationSectionInstanceErrors.InvalidIndex;

            return new ApplicationSectionInstance(id, stepInstanceId, templateSectionId, index);
        }

        // 4. THE GATEKEEPER: Spawn FieldValue through SectionInstance
        public Result<ApplicationFieldValue> AddNewFieldValue(Guid applicationId, Guid fieldDefinitionId)
        {
            // Business Rule: One value per field definition within this section instance
            if (_Fields.Any(f => f.FieldDefinitionId == fieldDefinitionId))
                return ApplicationSectionInstanceErrors.DuplicateField;

            // Call internal static Create on ApplicationFieldValue
            var fieldValueResult = ApplicationFieldValue.Create(
                Guid.NewGuid(),
                applicationId,
                this.Id, // Link to this Section Instance
                fieldDefinitionId
            );

            if (fieldValueResult.IsError) return fieldValueResult.TopError;

            _Fields.Add(fieldValueResult.Value);
            return fieldValueResult.Value;
        }
    }
}