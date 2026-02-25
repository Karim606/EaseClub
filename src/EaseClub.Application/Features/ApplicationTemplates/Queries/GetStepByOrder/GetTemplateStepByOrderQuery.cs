using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetStepByOrder
{
    public record GetTemplateStepByOrderQuery(Guid TemplateId,int Order) : IRequest<Result<StepDetailsDto>>;
}
