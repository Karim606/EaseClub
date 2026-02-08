using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public class MembershipPlan : AuditableEntity,IHaveClub
    {
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }

        public string Name { get; private set; }
        public string? Description { get; private set; }

        public decimal TotalPrice { get; private set; }
        public int DurationInDays { get; private set; }
        public bool IsActive { get; private set; } = true;

        private readonly List<PlanInstallmentTemplate> _InstallmentTemplates = new();
        public IReadOnlyList<PlanInstallmentTemplate> InstallmentTemplates => _InstallmentTemplates.AsReadOnly();

        private MembershipPlan() { } // EF

        private MembershipPlan(Guid id,Guid clubId, Guid membershipTypeId, string name, decimal totalPrice, int durationInDays):base(id)
        {
            
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            Name = name;
            TotalPrice = totalPrice;
            DurationInDays = durationInDays;
            IsActive = true;
        }

        public Result<Success> AddInstallmentTemplate(InstallmentTemplate template)
        {
           if( template == null)
                return MembershipPlanErrors.InstallmenttTemplateMustBeProvided;

            if (_InstallmentTemplates.Any(x => x.InstallmentTemplateId == template.Id))
                return MembershipPlanErrors.InstallmenttTemplateAlreadyAddedForThisPlan;

            var maxDueAfter = template.Installments.Max(i => i.DueAfterDays);

            if (maxDueAfter > DurationInDays)
                return MembershipPlanErrors.InstallmentTemplateExceedsPlanDuration;

            var planInstallmentTemplate = PlanInstallmentTemplate.Create(this.Id, template.Id);
            
               if(planInstallmentTemplate.IsSuccess)
                _InstallmentTemplates.Add(planInstallmentTemplate.Value);
                else
                     return planInstallmentTemplate.TopError;

               return Result.Success;
        }

        public Result<Success> RemoveInstallmentTemplate(Guid templateId)
        {
            var existingTemplate = _InstallmentTemplates.FirstOrDefault(x => x.InstallmentTemplateId == templateId);
            if (existingTemplate == null)
                return MembershipPlanErrors.InstallmentTemplateNotFoundInThisPlan;
            _InstallmentTemplates.Remove(existingTemplate);
            return Result.Success;
        }

        /// <summary>
        /// Generate membership installments for a specific membership
        /// </summary>
        public Result<List<MembershipInstallment>> GenerateMembershipInstallments(Guid membershipId, InstallmentTemplate template)
        {
            if (_InstallmentTemplates.All(x => x.InstallmentTemplateId != template.Id))
                return MembershipPlanErrors.InstallmentTemplateDoesntExist;

            var installmentsOfTemplate = template.Installments;

            if(installmentsOfTemplate == null)
                return MembershipPlanErrors.TemplateInstallmentsDontExist;

            var membershipInstallments = new List<MembershipInstallment>();
            decimal runningTotal = 0;

            for ( int i=0; i< installmentsOfTemplate.Count; i++)
            {
                var item = installmentsOfTemplate[i];

                decimal installmentAmount;

                if (i == installmentsOfTemplate.Count - 1)
                {
                    // Calculate the last one based on the remaining money balance
                    installmentAmount = TotalPrice - runningTotal;
                }
                else
                {
                    // Round to 2 decimal places for currency
                    installmentAmount = Math.Round((TotalPrice * item.PercentageOfAmount) / 100, 2);
                    runningTotal += installmentAmount;
                }


                var membershipInstallment = new MembershipInstallment(
                    membershipId,
                    ClubId,
                    item.OrderIndex,
                    installmentAmount,
                    DateTime.UtcNow.AddDays(item.DueAfterDays)
                    );

                 membershipInstallments.Add(membershipInstallment);
            }
            
            return membershipInstallments;
        }

        public static Result<MembershipPlan> Create(Guid id,Guid clubId, Guid membershipTypeId, string name, decimal totalPrice, int durationInDays)
        {
            if (string.IsNullOrWhiteSpace(name))
                return MembershipPlanErrors.MembershipPlanNameMustNotBeEmpty;
            if (totalPrice <= 0)
                return MembershipPlanErrors.MembershipPlanTotalPriceMustBeGreaterThanZero;
            if (durationInDays <= 0)
                return MembershipPlanErrors.MembershipPlanDurationMustBeGreaterThanZero;
            if (membershipTypeId == Guid.Empty)
                return MembershipPlanErrors.MembershipTypeIdMustBeProvided;
            if (clubId == Guid.Empty)
                return MembershipPlanErrors.ClubIdMustBeProvided;

            var membershipPlan = new MembershipPlan(id, clubId, membershipTypeId, name, totalPrice, durationInDays);
            return membershipPlan;
        }
    }
}
