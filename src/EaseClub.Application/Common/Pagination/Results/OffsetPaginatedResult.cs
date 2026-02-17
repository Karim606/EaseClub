using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination.Results
{
    public sealed class OffsetPaginatedResult<T> : PaginatedResult<T>
    {
        public int Page { get; init; }
        public int TotalPages { get; init; }
        public int TotalCount { get; init; }
    }
}
