using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common.ValueObjects
{

    public sealed record PhoneNumber
    {
        private static readonly Regex PhoneRegex =
            new(@"^(?:\+20|0)?(10|11|12|15)\d{8}$", RegexOptions.Compiled);

        public string Value { get; private set; }

        private PhoneNumber(string value)
        {
            Value = value;
        }

        public static Result<PhoneNumber> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Error.Validation(description:"Phone number cannot be empty.");

            value = value.Trim();

            if (!PhoneRegex.IsMatch(value))
                return Error.Validation(description: "Invalid Egyptian phone number format.");

            return new PhoneNumber(value);
        }

        public override string ToString() => Value;
    }

}
