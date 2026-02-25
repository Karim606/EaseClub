using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Errors
{
    public static class ApplicationStepErrors
    {
        public static Error ClubIdRequired = Error.Validation(
            code: "ApplicationStepDefinition.ClubId.Required",
            description: "A valid Club ID must be provided.");

        public static Error TitleRequired = Error.Validation(
            code: "ApplicationStepDefinition.Title.Required",
            description: "Step title cannot be empty.");

        public static Error CategoryRequired = Error.Validation(
            code: "ApplicationStepDefinition.Category.Required",
            description: "Step category (e.g., IDENTITY) is required.");

        public static Error SectionsRequired = Error.Validation(
            code: "ApplicationStepDefinition.Sections.Required",
            description: "A step definition must contain at least one section.");
        
        public static Error DuplicateSection = Error.Validation(
            code: "ApplicationStepDefinition.Section.Duplicate",
            description: "This section already exists within the step.");

        public static Error SectionRequired = Error.Validation(
            code: "ApplicationStepDefinition.Section.Null",
            description: "Cannot add a null section definition to the step.");

        public static Error TemplateIdRequired = Error.Validation(
            code: "ApplicationStepDefinition.TemplateId.Required",
            description: "The template association is required.");

        public static Error SectionDoesntExist = Error.Validation(
            code: "ApplicationStepDefinition.Section.Doesnt.Exist",
            description: "Section doesn't exist within step.");

        public static Error DuplicateSectionTitle = Error.Validation(
            code: "ApplicationStepDefinition.Duplicate.Section.Title",
            description: "Another section exists with same title within step.");

        public static Error InvalidSectionOrder =>
            Error.Validation("ApplicationStepDefintion.InvalidSectionOrder",
                "Section order is not valid.");
    }
}
