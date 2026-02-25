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

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.DeleteTemplate
{
    public record DeleteTemplateCommand(Guid ClubId, Guid TemplateId)
    : IRequest<Result<Success>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                auth => auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TemplateId, ClubId),
                nameof(ApplicationTemplateDefinition),
                TemplateId);
        }
    }
}
