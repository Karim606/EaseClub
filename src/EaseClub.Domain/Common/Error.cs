using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public readonly record struct  Error
    {
        public string Code { get; init; }
        public string Description { get; init; }
        public ErrorKind Type { get; init; }

        public Error(string code, string description,ErrorKind type)
        {
            Code = code;
            Description = description;
            Type = type;
        }
        public object ToLogObject() => new { Code, Description, Type = Type.ToString() };

        public static Error NotFound(string code = nameof(NotFound), string description = "Not found error") => new Error(code, description, ErrorKind.NotFound);
        public static Error Validation(string code = nameof(Validation), string description = "Validation error") => new Error(code, description, ErrorKind.Validation);
        public static Error Conflict(string code = nameof(Conflict), string description = "Conflict error") => new Error(code, description, ErrorKind.Conflict);
        public static Error Unexpected(string code = nameof(Unexpected), string description = "Unexpected error") => new Error(code, description, ErrorKind.Unexpected);
        public static Error Unauthorized(string code = nameof(Unauthorized), string description = "Unauthorized error") => new Error(code, description, ErrorKind.Unauthorized);
        public static Error Forbidden(string code = nameof(Forbidden), string description = "Forbidden error") => new Error(code, description, ErrorKind.Forbidden);
        public static Error Failure(string code = nameof(Failure), string description = "Failure error") => new Error(code, description, ErrorKind.Failure);
        public static Error Create(string code, string description, int type) => new Error(code, description, (ErrorKind)type);
    }
}

