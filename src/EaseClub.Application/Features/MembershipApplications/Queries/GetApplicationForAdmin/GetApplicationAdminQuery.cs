using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationForAdmin
{
    public record GetApplicationAdminQuery(Guid ApplicationId)
     : IRequest<Result<ApplicationAdminResponse>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, clubId) =>
                    await auth.DoesResourceBelongToClubAsync<MembershipApplication>(ApplicationId, clubId),
                nameof(MembershipApplication),
                ApplicationId
            );
        }
    }

    public record ApplicationAdminResponse(
    Guid Id,
    string TrackingNumber,
    ApplicationStatus Status,
    List<AdminStepDto> Steps);

    public record AdminStepDto(
        Guid Id,
        string Title,
        int Order,
        List<AdminSectionDto> Sections
    );

    public record AdminSectionDto(
        Guid Id,
        string Title,
        SectionIntent Intent,
        RepeatRule? RepeatRule,
        string? InstanceId,
        List<AdminFieldDto> Fields
    );

    public record AdminFieldDto(
        Guid Id,
        string Key,
        string Label,
        FieldType Type,
        string? Value,
        List<string>? AllowedValues,
        ValidationRuleSetSnapshot ValidationRules
    );

}
