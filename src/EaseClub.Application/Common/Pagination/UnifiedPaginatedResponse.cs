using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination
{
    public record UnifiedPaginatedResponse<T>(
    IReadOnlyList<T> Items,
    bool HasMore,
    int? Page = null,
    int? TotalCount = null,
    string? NextCursor = null
);
}
