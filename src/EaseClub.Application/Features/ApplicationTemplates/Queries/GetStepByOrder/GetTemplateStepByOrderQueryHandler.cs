using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetStepByOrder
{
    public class GetTemplateStepByOrderQueryHandler(ILogger<GetTemplateStepByOrderQueryHandler>logger,
        IApplicationTemplateQueryService queryService)
        :IRequestHandler<GetTemplateStepByOrderQuery, Result<StepQuery>>
    {


        public async Task<Result<StepQuery>> Handle(GetTemplateStepByOrderQuery request, CancellationToken ct)
        {
            var res = await queryService.GetTemplateStepByOrder(request.TemplateId, request.Order,ct);

            if (res.IsError) {
                logger.LogError("failed to retrieve step with order:{Order} that belongs to template with id:{TemplateId}",
                    request.Order, request.TemplateId);
                return res.TopError; 
            }
            return res;
        }
    }
}
