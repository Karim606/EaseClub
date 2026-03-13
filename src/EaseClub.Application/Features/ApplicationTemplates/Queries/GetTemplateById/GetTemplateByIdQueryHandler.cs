using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplateById
{
    public class GetTemplateByIdQueryHandler(
        IApplicationTemplateQueryService queryService) : IRequestHandler<GetTemplateByIdQuery, Result<TemplateTreeQuery>>
    {
        public async Task<Result<TemplateTreeQuery>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var template = await queryService.GetFullTemplateTreeAsync(request.TemplateId, cancellationToken);

            return template;
        }
    }
}
