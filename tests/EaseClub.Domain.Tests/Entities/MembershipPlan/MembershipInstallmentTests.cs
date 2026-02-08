using EaseClub.Domain.MembershipPlans;
using FluentAssertions;
using Xunit;

namespace EaseClub.Tests.Domain.MembershipPlans;

public class MembershipInstallmentTests
{
    private readonly Guid _validMembershipId = Guid.NewGuid();
    private readonly decimal _validAmount = 100.00m;
    private readonly int _validOrder = 0;
    // Set due date to tomorrow to pass factory validation
    private readonly DateTime _futureDueDate = DateTime.UtcNow.AddDays(1);

    #region Creation Tests

    [Fact]
    public void Create_ShouldSucceed_WhenDataIsValid()
    {
        // Act
        var result = MembershipInstallment.Create(_validMembershipId,Guid.NewGuid(),_validOrder, _validAmount, _futureDueDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(InstallmentStatus.Pending);
        result.Value.Amount.Should().Be(_validAmount);
    }

    [Fact]
    public void Create_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        // Act
        var result = MembershipInstallment.Create(_validMembershipId, Guid.NewGuid(), _validOrder, 0, _futureDueDate);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(MembershipInstallmentErrors.InstallmentAmountMustBeGreaterThanZero);
    }

    [Fact]
    public void Create_ShouldFail_WhenDueDateIsInThePast()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var result = MembershipInstallment.Create(_validMembershipId, Guid.NewGuid(), _validOrder, _validAmount, pastDate);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(MembershipInstallmentErrors.InstallmentDueDateMustBeInTheFuture);
    }

    #endregion

    #region Payment State Tests

    [Fact]
    public void MarkPaid_ShouldSucceed_WhenStatusIsPending()
    {
        // Arrange
        var installment = MembershipInstallment.Create(_validMembershipId, Guid.NewGuid(), _validOrder, _validAmount, _futureDueDate).Value;
        var invoiceId = Guid.NewGuid();

        // Act
        var result = installment.MarkPaid(invoiceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        installment.Status.Should().Be(InstallmentStatus.Paid);
        installment.InvoiceId.Should().Be(invoiceId);
    }

    [Fact]
    public void MarkPaid_ShouldFail_WhenAlreadyPaid()
    {
        // Arrange
        var installment = MembershipInstallment.Create(_validMembershipId, Guid.NewGuid(), _validOrder, _validAmount, _futureDueDate).Value;
        installment.MarkPaid(Guid.NewGuid());

        // Act
        var result = installment.MarkPaid(Guid.NewGuid());

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(MembershipInstallmentErrors.OnlyPendingInstallmentOrOverDueCanBePaid);
    }

    #endregion

    #region Overdue Logic Tests

    [Fact]
    public void MarkOverdue_ShouldFail_IfDateIsNotYetPassed()
    {
        // Arrange
        // Note: Creation allows future dates. MarkOverdue checks if we have reached that date.
        var installment = MembershipInstallment.Create(_validMembershipId, Guid.NewGuid(), _validOrder, _validAmount, _futureDueDate).Value;

        // Act
        var result = installment.MarkOverdue();

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(MembershipInstallmentErrors.NotOverDuedYet);
        installment.Status.Should().Be(InstallmentStatus.Pending);
    }

    
    [Fact]
    public void MarkOverdue_ShouldSucceed_WhenDueDateHasPassed()
    {
        // Arrange
        // We bypass the Factory 'Future' check by using the constructor directly (if public/internal)
        // or simulating time. 
        var dueDateInPast = DateTime.UtcNow.AddDays(-1);
        var installment = new MembershipInstallment(_validMembershipId, Guid.NewGuid(), _validOrder, _validAmount, dueDateInPast);

        // Act
        var result = installment.MarkOverdue();

        // Assert
        result.IsSuccess.Should().BeTrue();
        installment.Status.Should().Be(InstallmentStatus.Overdue);
    }

    #endregion
}