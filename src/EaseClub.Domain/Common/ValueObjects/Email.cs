using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common.ValueObjects
{
    using EaseClub.Domain.Common.Results;
    using System;
    

    public sealed record Email
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public string Value { get; private set; }

        private Email(string value)
        {

            Value = value;
        }

        public static Result<Email> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Error.Validation(description: "Email cannot be empty.");

            value = value.Trim().ToLower();

            if (!EmailRegex.IsMatch(value))
                return Error.Validation(description: "Invalid email format.");

            return new Email(value);
        }

        public override string ToString() => Value;
    }

}
