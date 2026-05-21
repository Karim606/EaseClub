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

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate
{
    public record CreateInstallmentTemplateCommand(
    Guid ClubId,
    string Name,
    int? NumOfInstallments,
    int?DurationInDays,
    List<InstallmentDto>? Installments) : IRequest<Result<Guid>>,IRequireClubAdmin;
}
