using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination.Parameters
{
    public abstract class PaginationParameters
    {
        public int Limit { get; init; } = 20;
        public string? Search { get; init; }
        public string? SortBy { get; init; } = "CreatedAt";
        public bool SortDesc { get; init; } = true;
    }
}
