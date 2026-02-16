using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class ApplicationSectionInstanceErrors
    {
        public static Error StepInstanceRequired =>
            Error.Validation("ApplicationSection.StepInstanceRequired",
                "StepInstanceId is required.");

        public static Error TemplateSectionRequired =>
            Error.Validation("ApplicationSection.TemplateSectionRequired",
                "TemplateSectionId is required.");

        public static Error InvalidIndex =>
            Error.Validation("ApplicationSection.InvalidIndex",
                "Index must be zero or greater.");

        public static Error DuplicateField =>
            Error.Conflict("ApplicationSection.DuplicateField",
                "Field already exists in this section instance.");
    }
}
