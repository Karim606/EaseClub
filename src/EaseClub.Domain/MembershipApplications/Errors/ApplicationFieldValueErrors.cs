using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class ApplicationFieldValueErrors
    {
        public static Error ApplicationRequired =>
            Error.Validation("ApplicationFieldValue.ApplicationRequired",
                "ApplicationId is required.");

        public static Error SectionInstanceRequired =>
            Error.Validation("ApplicationFieldValue.SectionInstanceRequired",
                "SectionInstanceId is required.");

        public static Error FieldDefinitionRequired =>
            Error.Validation("ApplicationFieldValue.FieldDefinitionRequired",
                "FieldDefinitionId is required.");

        public static Error InvalidStringValue =>
            Error.Validation("ApplicationFieldValue.InvalidStringValue",
                "String value cannot be empty.");

        public static Error InvalidJson =>
            Error.Validation("ApplicationFieldValue.InvalidJson",
                "Complex JSON value is invalid.");
    }
}
