using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination.Parameters
{
    public sealed class OffsetPaginationParameters : PaginationParameters
    {
        private readonly int _page = 1;

        public int Page
        {
            get => _page;
            init => _page = value < 1 ? 1 : value; // Safeguard: minimum page is 1
        }

        
        public int CalculatedOffset() => (Page - 1) * Limit;
    }
}
