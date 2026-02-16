using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Errors;

namespace EaseClub.Domain.MembershipApplications
{
    public class ApplicationFieldValue : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationFieldValue() { }

        // 2. Private Constructor: Locked down
        private ApplicationFieldValue(
            Guid id,
            Guid applicationId,
            Guid sectionInstanceId,
            Guid fieldDefinitionId) : base(id)
        {
            ApplicationId = applicationId;
            SectionInstanceId = sectionInstanceId;
            FieldDefinitionId = fieldDefinitionId;
        }

        public Guid ApplicationId { get; private set; }
        public Guid SectionInstanceId { get; private set; }
        public Guid FieldDefinitionId { get; private set; }

        public string? StringValue { get; private set; }
        public decimal? NumberValue { get; private set; }
        public DateTime? DateValue { get; private set; }
        public string? ComplexJson { get; private set; }

        #region Factory

        // 3. INTERNAL Factory: Only ApplicationSectionInstance can call this
        internal static Result<ApplicationFieldValue> Create(
            Guid id,
            Guid applicationId,
            Guid sectionInstanceId,
            Guid fieldDefinitionId)
        {
            if (applicationId == Guid.Empty)
                return ApplicationFieldValueErrors.ApplicationRequired;

            if (sectionInstanceId == Guid.Empty)
                return ApplicationFieldValueErrors.SectionInstanceRequired;

            if (fieldDefinitionId == Guid.Empty)
                return ApplicationFieldValueErrors.FieldDefinitionRequired;

            return new ApplicationFieldValue(
                id,
                applicationId,
                sectionInstanceId,
                fieldDefinitionId);
        }

        #endregion

        #region Setters (Public logic for updating values)

        public Result<Success> SetString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ApplicationFieldValueErrors.InvalidStringValue;

            ClearAll();
            StringValue = value;
            return Result.Success;
        }

        public Result<Success> SetNumber(decimal value)
        {
            ClearAll();
            NumberValue = value;
            return Result.Success;
        }

        public Result<Success> SetDate(DateTime value)
        {
            ClearAll();
            DateValue = value;
            return Result.Success;
        }

        public Result<Success> SetComplexJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return ApplicationFieldValueErrors.InvalidJson;

            ClearAll();
            ComplexJson = json;
            return Result.Success;
        }

        private void ClearAll()
        {
            StringValue = null;
            NumberValue = null;
            DateValue = null;
            ComplexJson = null;
        }

        #endregion
    }
}