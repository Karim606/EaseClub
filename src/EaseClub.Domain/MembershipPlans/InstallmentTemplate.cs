using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public class InstallmentTemplate : AuditableEntity,IHaveClub
    {
        public string Name { get; private set; }
        public Guid ClubId { get; private set; }
        public Club Club { get; private set; }

        private readonly List<Installment>_Installments = new();
        public  IReadOnlyList<Installment> Installments => _Installments.AsReadOnly();

        private readonly List<PlanInstallmentTemplate> _MembershipPlans = new();
        public IReadOnlyList<PlanInstallmentTemplate> MembershipPlans => _MembershipPlans.AsReadOnly();

        private InstallmentTemplate() { }

        private InstallmentTemplate(Guid id,Guid clubId, string name, List<Installment> installments):base(id)
        {

            _Installments = installments;
            Name = name;
            ClubId = clubId;
        }

        public static Result<InstallmentTemplate>Create(Guid id,Guid clubId,string name,int? numOfInstallments,int? durationInDays,
            List<Installment>? installments)
        {

            var generatedInstallments = new List<Installment>();

            // Generate installments based on numOfInstallments and Duration
            if (numOfInstallments != null && durationInDays!=null)
            {
                var resultOfCustomGeneratedList = GenerateCustomInstallmentsList(numOfInstallments.Value, durationInDays.Value);

                if(resultOfCustomGeneratedList.IsError)
                    return resultOfCustomGeneratedList.TopError;

                generatedInstallments = resultOfCustomGeneratedList.Value;
            }
            
            //Generate installments based on provided list
            else if (installments != null && installments.Any())
            {

                var validationOfInstallments = ValidateInstallments(installments);

                if (validationOfInstallments.IsError)
                    return validationOfInstallments.TopError;

                generatedInstallments = installments;
            }
            
            // Neither numOfInstallments nor installments provided
            else
            {
                return InstallmentTemplateErrors.EitherNumOfInstallmentsOrListOfInstallmentsMustBeProvided;
            }

            var template = new InstallmentTemplate(id,clubId,name,generatedInstallments);
            
            return template;
        }

        //---------------------------Private Methods--------------------------//

        private static Result<bool> ValidateInstallments(List<Installment> installments)
        {
            if (!installments.Any())
                return InstallmentTemplateErrors.EitherNumOfInstallmentsOrListOfInstallmentsMustBeProvided;

            var totalPercentage = installments.Sum(i => i.PercentageOfAmount);
            if (totalPercentage != 100m)
                return InstallmentTemplateErrors.TotalPercentageOfInstallmentsMustEqual100Percent;

            return true;
        }

        private static Result<List<Installment>> GenerateCustomInstallmentsList(int numOfInstallments,int durationInDays)
        {
            if (numOfInstallments <= 0)
                return InstallmentTemplateErrors.NumOfInstallmentMustBeGreaterThanZeroWhenProvided;

            if (durationInDays <= 0)
                return InstallmentTemplateErrors.DurationMustBeProvidedAndGreaterThanZeroWhenNumOfInstallmentsIsSpecified;

            decimal installmentPercentage = Math.Round(100m / numOfInstallments, 2);
            int daysBetweenInstallments = durationInDays / numOfInstallments;
            var generatedInstallments = new List<Installment>();

            for (int i = 0; i < numOfInstallments; i++)
            {
                int dueAfterDays = (i == numOfInstallments - 1) ? durationInDays : (daysBetweenInstallments) * i;
                // Adjust the last installment to ensure total is exactly 100%
                decimal percentage = (i == numOfInstallments - 1) ? 100m - installmentPercentage * (numOfInstallments - 1)
                    : installmentPercentage;

                var result = Installment.Create(percentage, dueAfterDays,i);
                if (result.IsError)
                    return result.TopError;

                generatedInstallments.Add(result.Value);
            }
            return generatedInstallments;
        }
        
    }

}
