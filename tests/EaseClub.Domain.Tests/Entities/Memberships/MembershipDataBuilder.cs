using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Tests.Entities.MembershipApplications;
using EaseClub.Domain.Tests.Entities.MembershipPlan;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.Memberships
{
    public static class MembershipDataBuilder
    {
        public static MembershipApplication CreateApprovedApplication()
        {
            // 1. Arrange domain objects
            var clubId = Guid.NewGuid();
            var membershipTypeId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var membershipType = MembershipType.Create(membershipTypeId, clubId, "Gold Type").Value;

            // Create the plan with values that match the Snapshot schema
            var plan = Domain.MembershipPlans.MembershipPlan.Create(
                planId,
                clubId,
                membershipTypeId,
                1,       // subscriptionValidityInYears
                5,       // maxFamilyMembers
                "Gold Plan",
                1000m,
                365      // maxPaymentPeriod
            ).Value;

            var installmentTemplate = InstallmentTemplateBuilder.CreateValidTemplate();
            plan.AddInstallmentTemplate(installmentTemplate);

            // 2. Synchronize Snapshot with the Plan values
            // Using your builder, ensure the snapshot reflects the domain plan
            var snapshot = ApplicationTestDataBuilder.CreateSnapshot(stepCount: 1);

            // Manually ensure the snapshot's MembershipPlanSnapshot matches the domain plan properties
            // (If your constructor forces a new, we may need to use reflection or 
            // update your builder to accept a Plan object)

            // 3. Create the Application
            var appResult = MembershipApplication.Create(
                Guid.NewGuid(),
                "APP-2026-001",
                snapshot,
                Guid.NewGuid(),
                clubId,
                plan,
                installmentTemplate,
                membershipType,
                Guid.NewGuid()
            );

            var app = appResult.Value;
            app.GetType().GetProperty("Status")?.SetValue(app, ApplicationStatus.Approved);
            app.GetPricePreview();

            app.TemplateSnapshot.InstallmentRules.Add(new InstallmentRuleSnapshot(0, 50, 0));
            app.TemplateSnapshot.InstallmentRules.Add(new InstallmentRuleSnapshot(1, 50, 30));
            return app;
        }

        public static Membership CreateSuspendedMembership()
        {
            var app = CreateApprovedApplication();
            return Membership.CreateFromApplication(app).Value;
        }

        public static Membership CreateActiveMembership()
        {
            var membership = CreateSuspendedMembership();
            membership.Activate();
            return membership;
        }
    }
}
