using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using FluentAssertions;
using Xunit;

namespace EaseClub.Domain.UnitTests.ApplicationTemplates;

public class ApplicationSectionTests
{
    private readonly ValidationRuleSet _dummyRules = ValidationRuleSet.Create(false).Value;

    [Fact]
    public void AddNewField_ShouldAddToList_WhenDataIsValid()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 0).Value;

        // Act
        var result = section.AddNewField("phone_number", FieldType.Text, _dummyRules, null, false, 0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        section.Fields.Should().HaveCount(1);
        section.Fields[0].Key.Should().Be("phone_number");
    }

    [Fact]
    public void AddNewField_ShouldShiftOrders_WhenInsertedAtExistingOrder()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 0).Value;
        // Add first field at order 0
        section.AddNewField("first", FieldType.Text, _dummyRules, null, false, 0);

        // Act - Add second field also at order 0
        var result = section.AddNewField("new_first", FieldType.Text, _dummyRules, null, false, 0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        section.Fields.First(f => f.Key == "new_first").Order.Should().Be(0);
        section.Fields.First(f => f.Key == "first").Order.Should().Be(1); // Shifted up
    }

    [Fact]
    public void RemoveField_ShouldCloseGap_ByDecrementingSubsequentOrders()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 0).Value;
        section.AddNewField("f1", FieldType.Text, _dummyRules, null, false, 0);
        var f2 = section.AddNewField("f2", FieldType.Text, _dummyRules, null, false, 1).Value;
        section.AddNewField("f3", FieldType.Text, _dummyRules, null, false, 2);

        // Act
        section.RemoveField(f2.Id);

        // Assert
        section.Fields.Should().HaveCount(2);
        section.Fields.First(f => f.Key == "f1").Order.Should().Be(0);
        section.Fields.First(f => f.Key == "f3").Order.Should().Be(1); // Shifted down from 2
    }

    [Fact]
    public void EvaluateRepeatRule_ShouldReturnZero_WhenSectionIsNotRepeatable()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 0, repeatRule: null).Value;

        // Act
        var result = section.EvaluateRepeatRule("3");

        // Assert
        // In your Result class, result.Value will be 0 if IsRepeatable is false
        result.Value.Should().Be(0);
    }

    [Fact]
    public void SetRepeatRule_ShouldUpdateRuleSuccessfully()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 0).Value;
        var rule = RepeatRule.Create("count", RepeatMode.ExactValue).Value;

        // Act
        var result = section.SetRepeatRule(rule);

        // Assert
        result.IsSuccess.Should().BeTrue();
        section.IsRepeatable.Should().BeTrue();
        section.RepeatRule.Should().Be(rule);
    }
}