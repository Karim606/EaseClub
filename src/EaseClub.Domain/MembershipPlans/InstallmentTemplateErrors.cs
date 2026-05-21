using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public static class InstallmentTemplateErrors
    {

        public static Error NumOfInstallmentMustBeGreaterThanZeroWhenProvided = 
            Error.Validation(code:"NumOfInstallments.Must.Be.GreaterThanZero.When.Provided",
                description: "Number of installments must be greater than zero When Provided instead of custom installments.");

        public static Error DurationMustBeProvidedAndGreaterThanZeroWhenNumOfInstallmentsIsSpecified = 
            Error.Validation(code: "Duration.Must.Be.Provided.And.GreaterThanZero.When.NumOfInstall",
                description: "Duration must be provided and greater than zero when number of installments is specified.");

        public static Error TotalPercentageOfInstallmentsMustEqual100Percent = 
            Error.Validation(code: "Total.Percentage.Of.Installments.Must.Equal.100Percent",
                description: "Total percentage of installments must equal 100%.");


        public static Error EitherNumOfInstallmentsOrListOfInstallmentsMustBeProvided = 
            Error.Validation(code: "Either.NumOfInstallments.Or.ListOfInstallments.Must.Be.Provided",
                description: "Either number of installments or a list of installments must be provided.");

        public static Error InstallmentTemplateGuidMustBeProvided = 
            Error.Validation(code: "InstallmentTemplate.Guid.Must.Be.Provided",
                description: "Installment template Guid must be provided.");
        public static Error MembershipGuidMustBeProvided = 
            Error.Validation(code: "InstallmentTemplate.MembershipGuid.Must.Be.Provided",
                description: "Membership Guid must be provided for the installment template.");
    }
}
