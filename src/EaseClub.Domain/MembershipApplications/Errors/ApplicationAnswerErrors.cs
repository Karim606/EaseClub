using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class ApplicationAnswerErrors
    {
        public static Error ApplicationRequired =>
            Error.Validation("ApplicationAnswer.ApplicationRequired",
                "ApplicationId is required.");

        public static Error FieldKeyRequired =>
            Error.Validation("ApplicationAnswer.FieldKeyRequired",
                "Field key is required.");

        public static Error FieldDefintionRequired => Error.Validation("ApplicationAnswer.FieldDefintionRequired",
            "FieldDefintion Id is required.");

    }
}
