using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EaseClub.Domain.Common.ValueObjects
{
    using System;
    

    public sealed record Email
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public string Value { get; init; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");

            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException("Invalid email format.");

            Value = value.Trim().ToLower();
        }

        public override string ToString() => Value;
    }

}
