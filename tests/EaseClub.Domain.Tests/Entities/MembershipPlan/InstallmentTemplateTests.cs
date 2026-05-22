using EaseClub.Domain.MembershipPlans;
using FluentAssertions;
using Xunit;

namespace EaseClub.Tests.Domain.MembershipPlans;

public class InstallmentTemplateTests
{
    private readonly Guid _validId = Guid.NewGuid();

    #region Manual List Validation Tests

    [Fact]
    public void Create_WithManualList_ShouldSucceed_WhenTotalIs100Percent()
    {
        // Arrange
        var installments = new List<Installment>
        {
            Installment.Create(40m, 0, 1).Value,
            Installment.Create(60m, 30, 2).Value
        };

        // Act
        var result = InstallmentTemplate.Create(_validId,Guid.NewGuid(),"abc", null, null, installments);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Installments.Should().HaveCount(2);
        result.Value.Installments.Sum(x => x.PercentageOfAmount).Should().Be(100m);
    }

    [Fact]
    public void Create_WithManualList_ShouldFail_WhenTotalIsNot100Percent()
    {
        // Arrange
        var installments = new List<Installment>
        {
            Installment.Create(50m, 0, 1).Value,
            Installment.Create(40m, 30, 2).Value // Total 90%
        };

        // Act
        var result = InstallmentTemplate.Create(_validId, Guid.NewGuid(),"abc", null, null, installments);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(InstallmentTemplateErrors.TotalPercentageOfInstallmentsMustEqual100Percent);
    }

    #endregion

    #region Auto-Generation Logic Tests

    [Theory]
    [InlineData(3, 90)]  // 3 installments over 90 days
    [InlineData(12, 365)] // 12 installments over a year
    public void Create_WithAutoGeneration_ShouldSucceed_AndNormalizePercentages(int count, int duration)
    {
        // Act
        var result = InstallmentTemplate.Create(_validId, Guid.NewGuid(),"abc", count, duration, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Installments.Should().HaveCount(count);

        // The most critical check: Does it sum to exactly 100 regardless of division math?
        result.Value.Installments.Sum(x => x.PercentageOfAmount).Should().Be(100m);

        // Check if the last installment ends exactly on the duration
        result.Value.Installments.Last().DueAfterDays.Should().Be(duration);
    }

    [Fact]
    public void Create_WithAutoGeneration_ShouldCorrectlyHandlePrimeNumbers()
    {
        // Arrange: 100 / 3 = 33.33333...
        int count = 3;
        int duration = 30;

        // Act
        var result = InstallmentTemplate.Create(_validId, Guid.NewGuid(), "abc", count, duration, null);

        // Assert
        var list = result.Value.Installments;
        list[0].PercentageOfAmount.Should().Be(33.33m);
        list[1].PercentageOfAmount.Should().Be(33.33m);
        list[2].PercentageOfAmount.Should().Be(33.34m); // Remainder adjustment
        list.Sum(x => x.PercentageOfAmount).Should().Be(100.00m);
    }

    #endregion

    #region Edge Case & Error Tests

    [Fact]
    public void Create_ShouldFail_WhenNoInputsProvided()
    {
        // Act
        var result = InstallmentTemplate.Create(_validId, Guid.NewGuid(), "abc", null, null, null);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(InstallmentTemplateErrors.EitherNumOfInstallmentsOrListOfInstallmentsMustBeProvided);
    }

    [Fact]
    public void Create_WithAutoGeneration_ShouldFail_WhenDurationIsMissing()
    {
        // Act: Providing count but missing duration
        var result = InstallmentTemplate.Create(_validId, Guid.NewGuid(), "abc", 5, null, null);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(InstallmentTemplateErrors.EitherNumOfInstallmentsOrListOfInstallmentsMustBeProvided);
    }

    #endregion
}
