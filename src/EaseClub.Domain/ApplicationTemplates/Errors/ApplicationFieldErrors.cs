using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Errors
{
    public static class ApplicationFieldErrors
    {
        public static Error SectionIdRequired => Error.Validation(
            code: "ApplicationField.SectionIdRequired",
            description: "Section id is required for the field.");
        public static Error TemplateIdRequired => Error.Validation(
            code: "ApplicationField.TemplateIdRequired",
            description: "Template id is required for the field.");

        public static Error ClubIdRequired => Error.Validation(
            code: "ApplicationField.ClubIdRequired",
            description: "Club ID is required for the field.");

        public static Error KeyRequired => Error.Validation(
            code: "ApplicationField.KeyRequired",
            description: "Field key must not be empty.");

        public static Error LabelRequired => Error.Validation(
            code: "ApplicationField.LabelRequired",
            description: "Field label must not be empty.");

        public static Error ValidationRulesRequired => Error.Validation(
            code: "ApplicationField.ValidationRulesRequired",
            description: "Validation rules must be provided.");

        public static Error SystemFieldCannotBeUpdated => Error.Conflict("ApplicationField.SystemField.Locked",
            "Cannot delete a system-critical field.");

    }
}

