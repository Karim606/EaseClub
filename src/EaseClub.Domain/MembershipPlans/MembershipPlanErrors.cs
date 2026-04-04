using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public static class MembershipPlanErrors
    {
        public static Error MembershipPlanNameMustNotBeEmpty =
            Error.Validation(code: "MembershipPlan.Name.Must.Not.Be.Empty",
                description: "Membership plan name must not be empty.");

        public static Error InstallmenttTemplateMustBeProvided =
            Error.Validation(code: "MembershipPlan.InstallmentTemplate.Must.Be.Provided",
                description: "Installment template must be provided.");

        public static Error MembershipPlanTotalPriceMustBeGreaterThanZero =
                    Error.Validation(code: "MembershipPlan.TotalPrice.Must.Be.GreaterThan.Zero",
                    description: "Membership plan total price must be greater than zero.");

        public static Error MembershipPlanDurationMustBeGreaterThanZero =
         Error.Validation(code: "MembershipPlan.Duration.Must.Be.GreaterThan.Zero",
                description: "Membership plan duration must be greater than zero.");

        public static Error MustHaveAtLeastOneInstallmentTemplate =
         Error.Validation(code: "MembershipPlan.Must.Have.At.Least.One.InstallmentTemplate",
                description: "Membership plan must have at least one installment template");

        public static Error MembershipTypeIdMustBeProvided =
         Error.Validation(code: "MembershipPlan.MembershipTypeId.Must.Be.Provided",
                description: "Membership type ID must be provided for the membership plan.");
        public static Error ClubIdMustBeProvided =
         Error.Validation(code: "MembershipPlan.ClubId.Must.Be.Provided",
                description: "Club ID must be provided for the membership plan.");

        public static Error InstallmentTemplateDoesntExist =
         Error.Validation(code: "MembershipPlan.InstallmentTemplate.Doesnt.Exist",
                description: "The specified installment template does not exist in the membership plan.");

        public static Error InstallmenttTemplateAlreadyAddedForThisPlan = 
         Error.Validation(code: "MembershipPlan.InstallmentTemplate.Already.Added.For.This.Plan",
                description: "The installment template has already been added for this membership plan.");

        public static Error InstallmentTemplateExceedsPlanDuration = 
         Error.Validation(code: "MembershipPlan.InstallmentTemplate.Exceeds.Plan.Duration",
                description: "The installment template duration exceeds the membership plan duration.");
        public static Error TemplateInstallmentsDontExist = Error.Validation(code: "MembershipPlan.Template.Installments.Dont.Exist",
                description: "The specified installment template does not have any installments.");
        
        public static Error InstallmentTemplateNotFoundInThisPlan =Error.Validation(
                code: "MembershipPlan.InstallmentTemplate.Not.Found.In.This.Plan",
                description: "The specified installment template was not found in this membership plan.");

        public static Error NameAlreadyExistsInClub = Error.Conflict(
                code: "MembershipPlan.Name.Already.Exists.In.Club",
                description: "A membership plan with the same name already exists in the specified club.");

        public static Error NotFound = Error.NotFound(
                code: "MembershipPlan.Not.Found",
                description: "The specified membership plan was not found.");

        public static Error AppTemplateRequired = Error.Validation("MembershipPlan.AppTemplate.Required", "App template is required when enrollment mode is ApplicationForm.");
        
        public static Error AppTemplateNotAllowed = Error.Validation("MembershipPlan.AppTemplate.NotAllowed", "App template is not allowed when enrollment mode is not ApplicationForm.");

        public static Error AppTemplateGuidMustBeProvided = Error.Validation("MembershipPlan.AppTemplate.Guid.Must.Be.Provided", "App template guid must be provided when enrollment mode is ApplicationForm.");

        public static Error AppTemplateInvalidForFamilyPlans = Error.Validation("MembershipPlan.AppTemplate.InvalidForFamilyPlans", "App template is not valid for family membership plans while template doesnt contain family Section.");
    }
}
