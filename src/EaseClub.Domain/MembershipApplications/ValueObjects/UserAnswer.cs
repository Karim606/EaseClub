using System;
using EaseClub.Domain.ApplicationTemplates;

namespace EaseClub.Domain.MembershipApplications.ValueObjects
{
    public record UserAnswer
    {
        public Guid FieldDefinitionId { get; init; }
        public string FieldKey { get; init; } = default!;
        public string? Value { get; init; }
        public string? InstanceId { get; init; } // Stable ID for repeated sections (e.g., a client-side generated UUID)
        public FieldType FieldType { get; init; }

        public UserAnswer(Guid fieldDefinitionId, string fieldKey, string? value, string? instanceId, FieldType fieldType)
        {
            FieldDefinitionId = fieldDefinitionId;
            FieldKey = fieldKey;
            Value = value;
            InstanceId = instanceId;
            FieldType = fieldType;
        }
    }
}
