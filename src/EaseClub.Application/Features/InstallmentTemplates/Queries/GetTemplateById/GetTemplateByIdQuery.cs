using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById
{
    public record GetInstallmentTemplateQuery( Guid Id)
    : IRequest<Result<InstallmentTemplateResponse>>;
}
