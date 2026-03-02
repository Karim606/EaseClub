using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Errors;

namespace EaseClub.Domain.MembershipApplications
{
    public class ApplicationAnswer : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationAnswer() { }

        // 2. Optimized Constructor
        private ApplicationAnswer(
            Guid id,
            Guid applicationId,
            Guid fieldDefintionId,
            string fieldKey,     // Use the Key from your JSON Snapshot
            string value,
            int instanceIndex) : base(id)
        {
            ApplicationId = applicationId;
            FieldDefinitionId = fieldDefintionId;   
            FieldKey = fieldKey;
            Value = value;
            InstanceIndex = instanceIndex;
        }

        public Guid ApplicationId { get; private set; }

        public Guid FieldDefinitionId { get; private set; }
        // We use FieldKey (from the Template Definition) to link answers 
        // to the fields defined in your JSON Snapshot.
        public string FieldKey { get; private set; }

        // Store everything as string. The UI knows how to parse it based on the Snapshot.
        public string Value { get; private set; }

        // 0 for standard fields, 1+ for repeatable section entries (e.g., Child 1, Child 2)
        public int InstanceIndex { get; private set; }

        #region Factory

        public static Result<ApplicationAnswer> Create(
            Guid applicationId,
            Guid fieldDefinitionId,
            string fieldKey,
            string value,
            int instanceIndex = 0)
        {
            if (applicationId == Guid.Empty)
                return ApplicationAnswerErrors.ApplicationRequired;

            if (fieldDefinitionId == Guid.Empty) return ApplicationAnswerErrors.FieldDefintionRequired;

            if (string.IsNullOrWhiteSpace(fieldKey))
                return ApplicationAnswerErrors.FieldKeyRequired;

            // id is generated here or passed in
            return new ApplicationAnswer(Guid.NewGuid(), applicationId,fieldDefinitionId, fieldKey, value, instanceIndex);
        }

        #endregion

        #region Logic

        internal void UpdateValue(string newValue)
        {
            // You can add validation logic here if needed
            Value = newValue;
        }

        internal void UpdateInstanceIndex(int newIndex)
        {
            InstanceIndex = newIndex;
        }

        #endregion
    }
}