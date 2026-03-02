using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
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
        // Arrange
        var snapshot = CreateBasicSnapshot(100);

        // Act
        var result = MembershipApplication.Create(
            Guid.NewGuid(), "TRK-123", snapshot, _userId, _clubId, Guid.NewGuid(), Guid.NewGuid(), _templateId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ApplicationStatus.Draft);
        result.Value.PricingState.Should().Be(PricingState.Estimated);
        result.Value.CompletedStepOrders.Should().BeEmpty();
    }

    [Fact]
    public void CompleteStep_ShouldWipeExistingAnswersForThatStepOnly()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var snapshot = CreateSnapshotWithStep(1, fieldId);
        var app = CreateDefaultApp(snapshot);

        var initialAnswer = ApplicationAnswer.Create(app.Id, fieldId, "key", "Initial", 0).Value;
        app.CompleteStep(1, new List<ApplicationAnswer> { initialAnswer });

        // Act - Re-complete the same step with a new value
        var newAnswer = ApplicationAnswer.Create(app.Id, fieldId, "key", "Updated", 0).Value;
        app.CompleteStep(1, new List<ApplicationAnswer> { newAnswer });

        // Assert
        app.Answers.Should().HaveCount(1);
        app.Answers.First().Value.Should().Be("Updated");
        app.CompletedStepOrders.Should().Contain(1);
    }

    [Fact]
    public void Submit_ShouldFail_IfDetailCountsDoNotMatchRepeatRuleDriver()
    {
        // Arrange: Rule says 2 instances required based on "guest_count"
        var repeatRule = RepeatRule.Create("guest_count", RepeatMode.ExactValue).Value;
        var driverFieldId = Guid.NewGuid();
        var detailFieldId = Guid.NewGuid();

        var snapshot = CreateSnapshotWithRepeatRule(1, driverFieldId, "guest_count", detailFieldId, repeatRule);
        var app = CreateDefaultApp(snapshot);

        // Provide 2 as driver, but only 1 detail instance
        var answers = new List<ApplicationAnswer>
        {
            ApplicationAnswer.Create(app.Id, driverFieldId, "guest_count", "2", 0).Value,
            ApplicationAnswer.Create(app.Id, detailFieldId, "guest_name", "John", 0).Value
        };
        app.CompleteStep(1, answers);

        // Act
        var result = app.Submit();

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Any(e => e.Code.Contains("SectionCountMismatch")).Should().BeTrue();
    }

    [Fact]
    public void Submit_ShouldLockPricingAndGenerateSummary()
    {
        // Arrange
        var snapshot = CreateBasicSnapshot(150);
        var app = CreateDefaultApp(snapshot);
        app.CompleteStep(1, new List<ApplicationAnswer>()); // Complete the only step

        // Act
        var result = app.Submit();

        // Assert
        result.IsSuccess.Should().BeTrue();
        app.Status.Should().Be(ApplicationStatus.Submitted);
        app.PricingState.Should().Be(PricingState.Locked);
        app.FinalPriceSummary.Should().NotBeNull();
        app.FinalPriceSummary!.BasePrice.Should().Be(150);
    }

    [Fact]
    public void GetPricePreview_ShouldReturnLockedSummary_WhenStateIsLocked()
    {
        // Arrange
        var snapshot = CreateBasicSnapshot(200);
        var app = CreateDefaultApp(snapshot);
        app.CompleteStep(1, new List<ApplicationAnswer>());
        app.Submit();

        // Act
        var preview = app.GetPricePreview();

        // Assert
        preview.IsSuccess.Should().BeTrue();
        preview.Value.TotalPrice.Should().Be(200);
        // This confirms it's returning the historical record, not recalculating
        app.PricingState.Should().Be(PricingState.Locked);
    }
    [Fact]
    public void CompleteStep_ShouldReturnPreviousStepRequired_WhenSkippingSteps()
    {
        // Arrange: Create a snapshot with 3 steps
        var snapshot = CreateSnapshotWithMultipleSteps(3);
        var app = CreateDefaultApp(snapshot);

        // Act: Try to complete Step 2 without finishing Step 1
        var result = app.CompleteStep(2, new List<ApplicationAnswer>());

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Any(e => e.Code.Contains("PreviousStepRequired")).Should().BeTrue();
        app.CurrentStepOrder.Should().Be(1); // Should still be at the start
    }

    [Fact]
    public void CompleteStep_ShouldAllowStepOne_RegardlessOfCompletedList()
    {
        // Arrange
        var snapshot = CreateSnapshotWithMultipleSteps(2);
        var app = CreateDefaultApp(snapshot);

        // Act
        var result = app.CompleteStep(1, new List<ApplicationAnswer>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        app.CompletedStepOrders.Should().Contain(1);
    }

    [Fact]
    public void MoveToStep_ShouldNotExceedTotalSteps_WhenMovingForward()
    {
        // Arrange: 2 steps total
        var snapshot = CreateSnapshotWithMultipleSteps(2);
        var app = CreateDefaultApp(snapshot);
        app.CompleteStep(1, new List<ApplicationAnswer>()); // Moves to step 2 automatically
        app.CompleteStep(2, new List<ApplicationAnswer>()); // Should try to move to 3

        // Act
        app.MoveToStep(3);

        // Assert
        app.CurrentStepOrder.Should().Be(2); // Capped at total steps
    }

    [Fact]
    public void MoveToStep_ShouldAllowMovingBack_ToAnyCompletedStep()
    {
        // Arrange
        var snapshot = CreateSnapshotWithMultipleSteps(3);
        var app = CreateDefaultApp(snapshot);
        app.CompleteStep(1, new List<ApplicationAnswer>());
        app.CompleteStep(2, new List<ApplicationAnswer>()); // CurrentStep is now 3

        // Act: User clicks the "Back" button to Step 1
        app.MoveToStep(1);

        // Assert
        app.CurrentStepOrder.Should().Be(1);
    }

    [Fact]
    public void MoveToStep_ShouldPreventMovingForward_BeyondNextAvailableStep()
    {
        // Arrange
        var snapshot = CreateSnapshotWithMultipleSteps(5);
        var app = CreateDefaultApp(snapshot);
        app.CompleteStep(1, new List<ApplicationAnswer>()); // Max allowed is now 2

        // Act: User tries to jump to Step 4
        app.MoveToStep(4);

        // Assert
        app.CurrentStepOrder.Should().Be(2); // Stay at the "High Water Mark"
    }

    // --- Helpers ---


    private ApplicationTemplateSnapshot CreateSnapshotWithMultipleSteps(int count)
    {
        var steps = Enumerable.Range(1, count).Select(i =>
            new StepSnapshot(Guid.NewGuid(), "Cat", $"Step {i}", i, new())
        ).ToList();

        return new ApplicationTemplateSnapshot(
            Guid.NewGuid(), "Multi-Step Template", 100,
            new List<PricingPolicySnapshot>(), steps);
    }

    private ApplicationTemplateSnapshot CreateBasicSnapshot(decimal baseFee)
    {
        return new ApplicationTemplateSnapshot(
            _templateId, "Test", baseFee,
            new List<PricingPolicySnapshot>(),
            new List<StepSnapshot> { new StepSnapshot(Guid.NewGuid(), "G", "S1", 1, new()) });
    }

    private ApplicationTemplateSnapshot CreateSnapshotWithStep(int order, Guid fieldId)
    {
        var validationRule = ValidationRuleSet.Create(false).Value;
        var field = new FieldSnapshot(fieldId, "k", "FieldLabel", FieldType.Text, validationRule, null, 0);
        var section = new SectionSnapshot(Guid.NewGuid(), "Sec", 0, null, new() { field });
        var step = new StepSnapshot(Guid.NewGuid(), "Cat", "Title", order, new() { section });

        return new ApplicationTemplateSnapshot(_templateId, "T", 100, new(), new() { step });
    }

    private ApplicationTemplateSnapshot CreateSnapshotWithRepeatRule(int order, Guid dId, string dKey, Guid fId, RepeatRule rule)
    {
        var validationRule = ValidationRuleSet.Create(false).Value;

        var driverField = new FieldSnapshot(dId, dKey,"FieldLabel",FieldType.Number, validationRule, null, 0);
        var detailField = new FieldSnapshot(fId, "detail","FieldLabel",FieldType.Text, validationRule, null, 0);

        var section = new SectionSnapshot(Guid.NewGuid(), "Guests", 0, rule, new() { detailField });
        var driverSection = new SectionSnapshot(Guid.NewGuid(), "Driver", 0, null, new() { driverField });

        var step = new StepSnapshot(Guid.NewGuid(), "C", "T", order, new() { driverSection, section });
        return new ApplicationTemplateSnapshot(_templateId, "T", 100, new(), new() { step });
    }

    private MembershipApplication CreateDefaultApp(ApplicationTemplateSnapshot s) =>
        MembershipApplication.Create(Guid.NewGuid(), "TRK", s, _userId, _clubId, Guid.NewGuid(), Guid.NewGuid(), _templateId).Value;
}