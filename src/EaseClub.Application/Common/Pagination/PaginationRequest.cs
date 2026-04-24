using EaseClub.Application.Common.Pagination.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination
{
    public class PaginationRequest
    {
        // Common
        public int Limit { get; init; } = 10;
        public bool SortDesc { get; init; } = false;

        // Offset Specific
        public int? Page { get; init; }

        // Cursor Specific
        public string? Cursor { get; init; }

        // Sort logic
        public string? SortBy { get; init; }

        // Helper to convert to your Domain/Application parameters
        public PaginationParameters ToParameters()
        {
            if (Page == null)
            {
                return new CursorPaginationParameters
                {
                    Cursor = Cursor,
                    Limit = Limit,
                    SortDesc = SortDesc
                };
            }
            else
            {

                return new OffsetPaginationParameters
                {
                    Page = Page ?? 1,
                    Limit = Limit,
                    SortBy = SortBy,
                    SortDesc = SortDesc
                };
            }
        }
    }
}
