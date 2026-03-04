using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Step.AddStep
{
    public record AddStepCommand(
    Guid ClubId,
    string Category,
    string Title,
    int Order) : IRequest<Result<Guid>>, IRequireClubAdmin,IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid TemplateId { get; init; }
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TemplateId, clubId),
                nameof(ApplicationTemplateDefinition),
                TemplateId
                );
        }
    }
}
