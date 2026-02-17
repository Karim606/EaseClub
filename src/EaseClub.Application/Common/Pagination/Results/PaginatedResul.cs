using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Pagination.Results
{
    public abstract class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

        public bool HasMore { get; init; }
    }
}
