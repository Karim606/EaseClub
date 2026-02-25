using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplates
{
    public class GetApplicationTemplatesQueryHandler(IApplicationTemplateQueryService queryService,
        ILogger<GetApplicationTemplatesQueryHandler> logger) :
        IRequestHandler<GetApplicationTemplatesQuery, Result<OffsetPaginatedResult<ApplicationTemplateDto>>>
    {
        public async Task<Result<OffsetPaginatedResult<ApplicationTemplateDto>>> Handle(GetApplicationTemplatesQuery request,
            CancellationToken cancellationToken)
        {
            var result = await queryService.GetTemplatesByClubAsync<OffsetPaginatedResult<ApplicationTemplateDto>>(request.ClubId,
                request.Parameters,
                cancellationToken);

            if(result.IsError) {
                logger.LogError("failed to get templates with ClubId:{ClubId}", request.ClubId);
                return result.TopError; 
            }

            return result;
        }
    }
}
