using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Section.UpdateSection
{
    public record UpdateSectionCommand(
    Guid ClubId,
    Guid SectionId, // Targeting the ID
    string Title,
    RepeatRule? RepeatRuleJson) : IRequest<Result<Success>>, IRequireClubAdmin, IRequireClubOwnershipValidation
{
    public IEnumerable<OwnershipRule> Rules()
    {
        yield return new OwnershipRule(
            (auth, clubId) => auth.CheckAppTemplateComponentsOwnership(typeof(ApplicationSectionDefinition), SectionId, clubId),
            nameof(ApplicationSectionDefinition),
            SectionId);
    }
}
}
