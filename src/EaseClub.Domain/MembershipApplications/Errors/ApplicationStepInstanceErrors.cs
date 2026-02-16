using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class ApplicationStepInstanceErrors
    {
        public static Error ApplicationIdRequired =>
            Error.Validation("ApplicationStepInstance.ApplicationIdRequired",
                "ApplicationId is required.");

        public static Error TemplateStepIdRequired =>
            Error.Validation("ApplicationStepInstance.TemplateStepIdRequired",
                "TemplateStepId is required.");

        public static Error StepLocked =>
            Error.Validation("ApplicationStepInstance.StepLocked",
                "Step is locked and cannot be modified.");

        public static Error DuplicateSectionInstance =>
            Error.Conflict("ApplicationStepInstance.DuplicateSectionInstance",
                "Section instance already exists in this step.");
    }
}
