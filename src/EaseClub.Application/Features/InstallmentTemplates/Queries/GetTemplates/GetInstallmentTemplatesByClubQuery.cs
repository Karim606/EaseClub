using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates
{
    public record GetInstallmentTemplatesByClubQuery(
    Guid ClubId,
    PaginationRequest Parameters) : IRequest<Result<UnifiedPaginatedResponse<TemplatesResponse>>>;
}
