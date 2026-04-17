using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplication
{
    public record GetApplicationQuery(Guid ApplicationId) : IRequest<Result<ApplicationUserResponse>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, clubId) =>  await (auth.DoesResourceBelongToCurrentUserAsync<MembershipApplication>(ApplicationId)),
                nameof(MembershipApplication),
                ApplicationId
                );
        }
    }

    public record ApplicationUserResponse(
    Guid Id,
    string TrackingNumber,
    int CurrentStepOrder,
    List<int> CompletedSteps,
    List<UserStepDto> Steps);

    public record UserStepDto(
        Guid Id,
        string Title,
        int Order,
        List<UserSectionDto> Sections
    );

    public record UserSectionDto(
        Guid Id,
        string Title,
        SectionIntent Intent,
        RepeatRule? RepeatRule,
        int InstanceIndex,
        List<UserFieldDto> Fields
    );

    public record UserFieldDto(
        Guid Id,
        string Key,
        string Label,
        FieldType Type,
        string? Value,
        List<string>?AllowedValues,
        ValidationRuleSetSnapshot ValidationRules
    );
}
