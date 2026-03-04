using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.RemoveField
{
    public record RemoveFieldCommand(Guid ClubId, Guid TemplateId, Guid SectionId, Guid FieldId)
    : IRequest<Result<Success>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TemplateId, clubId),
                nameof(ApplicationTemplateDefinition),
                TemplateId);
        }
    }
}
