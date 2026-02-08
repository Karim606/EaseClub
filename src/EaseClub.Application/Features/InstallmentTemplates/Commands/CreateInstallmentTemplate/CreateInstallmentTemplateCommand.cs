using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate
{
    public record CreateInstallmentTemplateCommand(
    string Name,
    int? NumOfInstallments,
    int?DurationInDays,
    List<Installment>? Installments) : IRequest<Result<Guid>>,IRequireClubAdmin
    {
        [JsonIgnore]
        public Guid ClubId { get; init; }
    }
}
