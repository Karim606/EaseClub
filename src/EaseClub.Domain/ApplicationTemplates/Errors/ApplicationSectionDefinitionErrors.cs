using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Errors
{
    public static class ApplicationSectionDefinitionErrors
    {
        public static Error DuplicateField = Error.Validation(
            code: "SectionDefinition.Field.Duplicate",
            description: "This field already exists within the section.");

        public static Error FieldRequired = Error.Validation(
            code: "SectionDefinition.Field.Null",
            description: "Cannot add a null field definition to the section.");


        public static Error FieldDoesntExist = Error.Validation(
            code: "SectionDefinition.Field.Doesnt.Exist",
            description: "Field doesn't exist within the section.");

        public static Error StepIdRequired =>
            Error.Validation("ApplicationSection.StepIdRequired",
                "StepId is required.");

        public static Error TitleRequired =>
            Error.Validation("ApplicationSection.TitleRequired",
                "Section title is required.");

        public static Error DuplicateFieldKey => Error.Validation(
            code: "ApplicationSection.Duplicate.Key",
            description: "Another field with same key exists within section.");

        public static Error InvalidFieldOrder => Error.Validation(
            code: "ApplicationSection.InvalidFieldOrder",
            description: "Field order is invalid.");
    }
}
