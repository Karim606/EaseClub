using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Errors
{
    public static class ApplicationTemplateDefinitionErrors
    {
        public static Error ClubIdRequired => Error.Validation(
            code: "ApplicationTemplateDefinition.ClubIdRequired",
            description: "Club ID is required to create a template.");

        public static Error MembershipTypeIdRequired => Error.Validation(
            code: "ApplicationTemplateDefinition.MembershipTypeIdRequired",
            description: "Membership type ID is required to create a template.");

        public static Error TemplateStepsRequired => Error.Validation(
            code: "ApplicationTemplateDefinition.TemplateStepsRequired",
            description: "At least one step is required to create a template.");

        public static Error InvalidPreviousTemplateId => Error.Validation(
            code: "ApplicationTemplateDefinition.InvalidPreviousTemplateId",
            description: "Previous template ID cannot be the same as new template ID.");

        public static Error StepCantBeNull => Error.Validation(
            code: "ApplicationTemplateDefinition.Step.Cant.Be.Null",
            description: "Step must be provided");

        public static Error SameStepCantBeAddedTwice => Error.Validation(
            code: "ApplicationTemplateDefinition.Same.Step.Cant.Be.Added.Twice",
            description:"Same Step Cant Be Added Twice");

        public static Error ThisStepBelongsToAnotherTemplate => Error.Validation(
            code: "ApplicationTemplateDefinition.This.Step.Belongs.To.Another.Template",
            description: "This Step Belongs To Another Template");

        public static Error StepIdRequired = Error.Validation(
            code: "ApplicationTemplateDefinition.StepId.Required",
            description: "A step definition must be linked to the template.");

        public static Error StepDoesntExist = Error.Validation(
            code: "ApplicationTemplateDefinition.Step.Doesnt.Exist",
            description: "Step doesn't exist within template.");

        public static Error InvalidName = Error.Validation(
            code: "ApplicationTemplateDefinition.Invalid.Name",
            description: "Name Must Not Be Empty Or Null");

        public static Error DuplicateStepTitle = Error.Validation(
            code: "ApplicationTemplateDefinition.Duplicate.Step.Title",
            description: "Another step exists with same title within template.");


    }
}
