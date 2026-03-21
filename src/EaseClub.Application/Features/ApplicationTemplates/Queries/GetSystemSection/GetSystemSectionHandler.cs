using EaseClub.Application.Common.Dtos;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetSystemSection
{
    public class GetSystemSectionHandler : IRequestHandler<GetSystemSectionQuery, Result<SystemSectionDto>>
    {
        public async Task<Result<SystemSectionDto>> Handle(GetSystemSectionQuery request, CancellationToken cancellationToken)
        {
            var section = SystemSectionRegistry.Get(request.Intent);

            var dto = SystemSectionDto.FromDomain(section);

            return dto;
        }
    }

}
