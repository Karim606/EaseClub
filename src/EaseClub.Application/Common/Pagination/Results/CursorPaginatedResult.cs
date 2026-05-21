using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination.Results
{
    public sealed class CursorPaginatedResult<T> : PaginatedResult<T>
    {
        public string? NextCursor { get; init; }
    }
}
