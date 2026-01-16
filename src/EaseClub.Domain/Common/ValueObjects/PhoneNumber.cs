using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EaseClub.Domain.Common.ValueObjects
{

    public sealed record PhoneNumber
    {
        private static readonly Regex PhoneRegex =
            new(@"^(?:\+20|0)?(10|11|12|15)\d{8}$", RegexOptions.Compiled);

        public string Value { get; init; }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be empty.");

            value = value.Trim();
            if (!PhoneRegex.IsMatch(value))
                throw new ArgumentException("Invalid Egyptian phone number format.");

            Value = value;
        }

        public override string ToString() => Value;
    }

}
