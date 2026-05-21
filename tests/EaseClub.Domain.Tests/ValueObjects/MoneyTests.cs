using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Common.ValueObjects.Errors;
using FluentAssertions;
using Xunit;

namespace EaseClub.Domain.UnitTests.Common.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Of_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Act
        var result = Money.Of(100, "EGP");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100);
        result.Value.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Of_ShouldNormalizeCurrency_ToUpper()
    {
        // Act
        var result = Money.Of(50, "egp");

        // Assert
        result.Value.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Add_ShouldReturnSum_WhenCurrenciesMatch()
    {
        // Arrange
        var m1 = Money.Of(10, "EGP").Value;
        var m2 = Money.Of(20, "EGP").Value;

        // Act
        var result = m1.Add(m2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(30);
    }

    [Fact]
    public void Add_ShouldFail_WhenCurrenciesDoNotMatch()
    {
        // Arrange
        var egp = Money.Of(10, "EGP").Value;
        var usd = Money.Of(10, "USD").Value;

        // Act
        var result = egp.Add(usd);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(MoneyErrors.CantOperateOnDifferentCurrrencies);
    }

    [Fact]
    public void Subtract_ShouldFail_WhenResultIsNegative()
    {
        // Arrange
        var m1 = Money.Of(10, "EGP").Value;
        var m2 = Money.Of(20, "EGP").Value;

        // Act
        var result = m1.Subtract(m2);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(MoneyErrors.MoneyAmountCantBeNonNegative);
    }

    [Fact]
    public void Money_ShouldHaveValueObjectEquality()
    {
        // Arrange
        var m1 = Money.Of(100, "EGP").Value;
        var m2 = Money.Of(100, "EGP").Value;

        // Assert
        // Records in C# compare by value, not by reference!
        m1.Should().Be(m2);
        (m1 == m2).Should().BeTrue();
    }
}