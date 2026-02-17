using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination.Parameters
{
    public sealed class CursorPaginationParameters : PaginationParameters
    {
        public string? Cursor { get; init; }
    }
}
