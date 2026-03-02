using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries
{
    public record ApplicationResponse(
    Guid Id,
    string TrackingNumber,
    int CurrentStepOrder,
    List<int> CompletedSteps,
    ApplicationTemplateSnapshot Template,
    List<AnswerDto> Answers,
    PricingResult? CurrentPrice);

    public record AnswerDto(Guid FieldId, string Key, string Value, int InstanceIndex = 0);
}
