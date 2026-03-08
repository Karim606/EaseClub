using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.UpdateTemplateList
{
   public record UpdateInstallmentListCommand(
    Guid TemplateId, 
    List<InstallmentDto> Installments) : IRequest<Result<Success>>,IRequireClubOwnershipValidation
    {

        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth,clubId)=> auth.DoesResourceBelongToClubAsync<InstallmentTemplate>(TemplateId,clubId),
                nameof(InstallmentTemplate), TemplateId);
        }
    }
}
