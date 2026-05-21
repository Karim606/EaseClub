using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common
{
    public abstract record BaseGetQuery<TDto, TPaginatedResult>(
     PaginationParameters Parameters
    ) : IRequest<TPaginatedResult>
     where TPaginatedResult : PaginatedResult<TDto>;
}
