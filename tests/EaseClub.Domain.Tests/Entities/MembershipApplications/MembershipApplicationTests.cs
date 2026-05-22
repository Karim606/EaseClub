using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.PricingPolices;
using EaseClub.Domain.Tests.Entities.MembershipApplications;
using FluentAssertions;
using Xunit;

namespace EaseClub.Tests.Domain.MembershipApplications;

public class MembershipApplicationTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clubId = Guid.NewGuid();
    private readonly Guid _templateId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldInitializeWithDraftStatusAndEstimatedPricing()
    {
        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1);

        var result = CreateApp(snapshot);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ApplicationStatus.Draft);
        result.Value.PricingState.Should().Be(PricingState.Estimated);
        result.Value.CompletedStepOrders.Should().BeEmpty();
    }

    [Fact]
    public void CompleteStep_ShouldWipeExistingAnswersForThatStepOnly()
    {
        var fieldId = Guid.NewGuid();
        var field = ApplicationTestDataBuilder.CreateField(fieldId, "key", FieldType.Text);
        var section = ApplicationTestDataBuilder.CreateSection("Sec", 0, null, new() { field });
        var step = ApplicationTestDataBuilder.CreateStepWithSection(1, new() { section });
        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1, new() { step });

        var app = CreateApp(snapshot).Value;

        app.CompleteStep(1, new() { new UserAnswer(fieldId, "key", "Initial", null, FieldType.Text) });
        app.CompleteStep(1, new() { new UserAnswer(fieldId, "key", "Updated", null, FieldType.Text) });

        app.Answers.Should().HaveCount(1);
        app.Answers.First().Value.Should().Be("Updated");
    }

    [Fact]
    public void Submit_ShouldFail_IfDetailCountsDoNotMatchRepeatRuleDriver()
    {
        var rule = RepeatRule.Create(2, RepeatMode.ExactValue).Value;
        var driverFieldId = Guid.NewGuid();
        var detailFieldId = Guid.NewGuid();

        var driverField = ApplicationTestDataBuilder.CreateField(driverFieldId, "guest_count", FieldType.Number);
        var detailField = ApplicationTestDataBuilder.CreateField(detailFieldId, "guest_name", FieldType.Text);

        var section = ApplicationTestDataBuilder.CreateSection("Guests", 1, rule, new() { detailField });
        var driverSection = ApplicationTestDataBuilder.CreateSection("Driver", 0, null, new() { driverField });

        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1, new() {
            ApplicationTestDataBuilder.CreateStepWithSection(1, new() { driverSection, section })
        });

        var app = CreateApp(snapshot).Value;
        var completeResult = app.CompleteStep(1, new List<UserAnswer>
        {
            new(driverFieldId, "guest_count", "2", null, FieldType.Number),
            new(detailFieldId, "guest_name", "John", null, FieldType.Text)
        });

        completeResult.IsError.Should().BeTrue();
        completeResult.Errors.Any(e => e.Code == "Application.InvalidRepeatCount").Should().BeTrue();
    }

    [Fact]
    public void Submit_ShouldLockPricingAndGenerateSummary()
    {
        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1, baseFee: 150);
        var app = CreateApp(snapshot).Value;

        app.CompleteStep(1, new List<UserAnswer>());
        var result = app.Submit();

        result.IsSuccess.Should().BeTrue();
        app.Status.Should().Be(ApplicationStatus.Submitted);
        app.FinalPriceSummary!.BasePrice.Should().Be(150);
    }

    [Fact]
    public void GetPricePreview_ShouldReturnLockedSummary_WhenStateIsLocked()
    {
        // Arrange: Use builder for snapshot with specific base fee
        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1, baseFee: 200);
        var app = CreateApp(snapshot).Value;

        app.CompleteStep(1, new List<UserAnswer>());
        app.Submit();

        // Act
        var preview = app.GetPricePreview();

        // Assert
        preview.IsSuccess.Should().BeTrue();
        preview.Value.TotalPrice.Should().Be(200);
        app.PricingState.Should().Be(PricingState.Locked);
    }

    [Fact]
    public void AddReview_ShouldApproveApplication_AndRaiseEvent()
    {
        // Arrange
        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1);
        var app = CreateApp(snapshot).Value;
        app.CompleteStep(1, new());
        app.Submit(); // Moves to Submitted

        var review = ApplicationReview.Create(Guid.NewGuid(), app.Id, Guid.NewGuid(),DecisionsAboutApplication.Approved, "Looks good", "Admin").Value;

        // Act
        var result = app.AddReview(review);

        // Assert
        result.IsSuccess.Should().BeTrue();
        app.Status.Should().Be(ApplicationStatus.Approved);
        app.DomainEvents.Should().ContainSingle(e => e is ApplicationApprovedEvent);
    }

    [Fact]
    public void GetPaymentSchedule_ShouldReturnError_IfSubmittedInvalidly()
    {
        // Arrange: Create an app, but don't complete steps to trigger pricing errors
        var snapshot = ApplicationTestDataBuilder.CreateSnapshot(1);
        var app = CreateApp(snapshot).Value;

        // Act
        var result = app.GetPaymentSchedule();

        // Assert
        result.IsError.Should().BeTrue();
    }

    // --- Helpers ---



    private Result<MembershipApplication> CreateApp(ApplicationTemplateSnapshot snapshot)
    {
        // 1. Create the MembershipType
        var membershipTypeId = Guid.NewGuid();
        var membershipType = MembershipType.Create(membershipTypeId,_clubId, "Premium Type").Value;

        // 2. Create the InstallmentTemplate using its factory
        var installmentTemplate = InstallmentTemplate.Create(
            Guid.NewGuid(),
            _clubId,
            "Monthly Plan",
            numOfInstallments: 12,
            durationInDays: 28,
            installments: null // Uses internal generation logic
        ).Value;

        // 3. Create the MembershipPlan using its factory
        var plan = MembershipPlan.Create(
            Guid.NewGuid(),
            _clubId,
            membershipTypeId,
            mode: EnrollmentMode.ApplicationForm,
            templateId: _templateId,
            subscriptionValidityInYears: 1,
            maxFamilyMembers: 5,
            name: "Gold Plan",
            totalPrice: 1000m,
            maxPaymentPeriod: 30,
            renewPrice: 1000m,
            installmentsAllowedInRenewal: true,
            paymentMode: PaymentMode.Installments
        ).Value;

        // Note: If MembershipPlan expects a list of templates internally, 
        // ensure you add the template to the plan or its collection before this point.
        // Add this to satisfy your domain validation:
        plan.AddInstallmentTemplate(installmentTemplate);
        // 4. Create the Application
        return MembershipApplication.Create(
            Guid.NewGuid(),
            "TRK-123",
            snapshot,
            _userId,
            _clubId,
            plan,
            installmentTemplate,
            membershipType,
            _templateId);
    }
}
