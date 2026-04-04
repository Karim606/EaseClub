using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipTypes;
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
        public MembershipType MembershipType { get; private set; }
        public EnrollmentMode EnrollmentMode { get; private set; }
        public Guid? ApplicationTemplateId { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }

        public decimal TotalPrice { get; private set; }
        public int MaxPaymentPeriodInDays { get; private set; }
        public int SubscriptionValidityInYears { get; private set; }
        public int MaxFamilyMembers { get; private set; }
        public bool IsActive { get; private set; } = true;

        private readonly List<PlanInstallmentTemplate> _InstallmentTemplates = new();
        public IReadOnlyList<PlanInstallmentTemplate> InstallmentTemplates => _InstallmentTemplates.AsReadOnly();

        private MembershipPlan() { } // EF

        private MembershipPlan(Guid id,Guid clubId, Guid membershipTypeId,int subscriptionValidityInYears,int maxFamilyMembers, string name,
            decimal totalPrice, int maxPaymentPeriod,EnrollmentMode mode) :base(id)
        {
            
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            Name = name;
            TotalPrice = totalPrice;
            MaxPaymentPeriodInDays = maxPaymentPeriod;
            SubscriptionValidityInYears = subscriptionValidityInYears;
            MaxFamilyMembers = maxFamilyMembers;
            IsActive = true;
            EnrollmentMode = mode;
        }

        public Result<Success> AddInstallmentTemplate(InstallmentTemplate template)
        {
           if( template == null)
                return MembershipPlanErrors.InstallmenttTemplateMustBeProvided;

            if (_InstallmentTemplates.Any(x => x.InstallmentTemplateId == template.Id))
                return MembershipPlanErrors.InstallmenttTemplateAlreadyAddedForThisPlan;

            var maxDueAfter = template.Installments.Max(i => i.DueAfterDays);

            if (maxDueAfter > MaxPaymentPeriodInDays)
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

      
        public static Result<MembershipPlan> Create(Guid id,Guid clubId, Guid membershipTypeId,
            EnrollmentMode mode,Guid? templateId,
            int subscriptionValidityInYears,int maxFamilyMembers,string name, decimal totalPrice, int maxPaymentPeriod)
        {
            if (string.IsNullOrWhiteSpace(name))
                return MembershipPlanErrors.MembershipPlanNameMustNotBeEmpty;
            if (mode == EnrollmentMode.ApplicationForm && templateId == null)
                return MembershipPlanErrors.AppTemplateRequired;

            // Invariant: Other modes must NOT have a template
            if (mode != EnrollmentMode.ApplicationForm && templateId != null)
                return MembershipPlanErrors.AppTemplateNotAllowed;

            if (totalPrice <= 0)
                return MembershipPlanErrors.MembershipPlanTotalPriceMustBeGreaterThanZero;
            if (maxPaymentPeriod <= 0)
                return MembershipPlanErrors.MembershipPlanDurationMustBeGreaterThanZero;
            if (subscriptionValidityInYears <= 0)
                return Error.Validation(description:"MembershipPlan.SubscriptionValidityInYearsMustBeGreaterThanZero");
            if (maxFamilyMembers < 0)
                return Error.Validation(description:"MembershipPlan.MaxFamilyMembersMustBeNonNegative");

            if (membershipTypeId == Guid.Empty)
                return MembershipPlanErrors.MembershipTypeIdMustBeProvided;
            if (clubId == Guid.Empty)
                return MembershipPlanErrors.ClubIdMustBeProvided;

            var membershipPlan = new MembershipPlan(id, clubId, membershipTypeId, subscriptionValidityInYears,maxFamilyMembers, name, totalPrice, maxPaymentPeriod,mode);

            membershipPlan.ApplicationTemplateId = templateId;
            return membershipPlan;
        }

        public Result<Success> Update(
        string name,
        string? description,
        decimal totalPrice,
    IEnumerable<InstallmentTemplate> newTemplates)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(name))
                return MembershipPlanErrors.MembershipPlanNameMustNotBeEmpty;
            if (totalPrice <= 0)
                return MembershipPlanErrors.MembershipPlanTotalPriceMustBeGreaterThanZero;

            // 2. Metadata Assignment
            Name = name;
            Description = description;
            TotalPrice = totalPrice;

            // 3. Sync Installment Templates (Reconciliation)
            var incomingIds = newTemplates.Select(t => t.Id).ToHashSet();

            // Remove templates not in the new list
            _InstallmentTemplates.RemoveAll(it => !incomingIds.Contains(it.InstallmentTemplateId));

            // Add only truly new templates
            foreach (var template in newTemplates)
            {
                if (_InstallmentTemplates.Any(it => it.InstallmentTemplateId == template.Id))
                    continue;

                // Reuse existing validation logic
                var addResult = AddInstallmentTemplate(template);
                if (addResult.IsError) return addResult.TopError;
            }

            return Result.Success;
        }

        public bool SupportsTemplate(Guid templateId)
        {
            return _InstallmentTemplates.Any(it => it.InstallmentTemplateId == templateId);
        }
        public Result<Success> AssignApplicationTemplate(Guid templateId)
        {
            if(EnrollmentMode != EnrollmentMode.ApplicationForm) return MembershipPlanErrors.AppTemplateNotAllowed;

            if (templateId == Guid.Empty)
                return MembershipPlanErrors.AppTemplateGuidMustBeProvided;

            ApplicationTemplateId = templateId;
            return Result.Success;
        }

        public MembershipPlanSnapshot ToSnapshot() => MembershipPlanSnapshot.FromDomain(this);
    }
}
