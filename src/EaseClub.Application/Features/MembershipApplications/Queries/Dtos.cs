using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries
{
    public record ApplicationResponse(
    Guid ApplicationId,
    string Status,
    object TemplateStructure, // The parsed JSON Snapshot (Steps/Sections/Fields)
    List<AnswerDto> Answers
);

    public record AnswerDto(
        Guid FieldDefinitionId,
        string Value,
        int InstanceIndex
    );
}
