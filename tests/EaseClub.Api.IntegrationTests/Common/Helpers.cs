using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Api.IntegrationTests.Common
{
    public  class Helpers
    {
        private readonly AppDbContext _context;
        public Helpers(AppDbContext context)
        {
            _context = context;
        }

        public  async Task<(Guid templateId, Guid stepId, Guid sectionId, Guid fieldId)> 
            CreateFullTemplateHierarchyAsync(Guid clubId)
        {
            // 1. Create Template
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), clubId, "Full Template").Value;

            // 2. Build steps/sections/fields snapshots
            var fieldId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var stepId = Guid.NewGuid();

            var rules = new ValidationRuleSetSnapshot(
                isRequired: true,
                minLength: null,
                maxLength: null,
                minValue: null,
                maxValue: null,
                minDate: null,
                maxDate: null
            );

            var field = new FieldSnapshot(
                id: fieldId,
                key: "full_name",
                label: "Full Name",
                fieldType: FieldType.Text,
                validationRules: rules,
                visibilityCondition: null,
                allowedValues: null,
                order: 1,
                isSystemField: false
            );

            var section = new SectionSnapshot(
                id: sectionId,
                title: "Details",
                order: 1,
                repeatRule: null,
                intent: SectionIntent.General,
                fields: new List<FieldSnapshot> { field }
            );

            var step = new StepSnapshot(
                id: stepId,
                title: "Step 1",
                order: 1,
                sections: new List<SectionSnapshot> { section }
            );

            template.UpdateSteps(new List<StepSnapshot> { step });

            await _context.ApplicationTemplateDefinitions.AddAsync(template);
            await _context.SaveChangesAsync();

            return (template.Id, stepId, sectionId, fieldId);
        }

        public async Task<MembershipType> CreateMembershipTypeAsync(Guid clubId, string name = "Test Membership")
        {
            var membershipType = MembershipType.Create(
                Guid.NewGuid(),
                clubId,
                name
            ).Value;

            await _context.MembershipTypes.AddAsync(membershipType);

            await _context.SaveChangesAsync();

            return membershipType;
        }

        public async Task<MembershipPlan> CreateMembershipPlanAsync(Guid clubId, Guid membershipTypeId, string name = "Test Plan", EnrollmentMode enrollmentMode = EnrollmentMode.ApplicationForm, Guid? templateId = null)
        {
            var membershipPlan = MembershipPlan.Create(
                Guid.NewGuid(),
                clubId,
                membershipTypeId,
                enrollmentMode,
                templateId,
                1, // subscriptionValidityInYears
                0, // maxFamilyMembers
                name,
                100, // totalPrice
                365, // maxPaymentPeriod
                80, // renewPrice
                false, // installmentsAllowedInRenewal
                PaymentMode.Cash
            ).Value;

            await _context.MembershipPlans.AddAsync(membershipPlan);
            await _context.SaveChangesAsync();

            return membershipPlan;
        }
    }
}
