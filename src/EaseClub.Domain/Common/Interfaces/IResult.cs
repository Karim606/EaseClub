using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common.Interfaces
{
    public interface IResult
    {
        IReadOnlyList<Error>? Errors {get; }
        bool IsSuccess { get; }
    }
    public interface  IResult<T> : IResult
    {
        T Value { get; }
    }
}
