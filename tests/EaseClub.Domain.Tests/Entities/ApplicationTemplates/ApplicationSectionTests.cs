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
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var title = "Emergency Contacts";

        // Act - This works because of InternalsVisibleTo
        var result = ApplicationSectionDefinition.Create(id, stepId, title, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(title);
        result.Value.Fields.Should().BeEmpty();
    }

    [Fact]
    public void AddNewField_ShouldAddToList_WhenDataIsValid()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 1).Value;

        // Act
        var result = section.AddNewField("phone_number", FieldType.Text, _dummyRules, null, false, true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        section.Fields.Should().HaveCount(1);
        section.Fields[0].Key.Should().Be("phone_number");
    }

    [Fact]
    public void AddNewField_ShouldFail_WhenKeyIsDuplicate()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 1).Value;
        section.AddNewField("email", FieldType.Text, _dummyRules, null, false, true);

        // Act
        var result = section.AddNewField("email", FieldType.Text, _dummyRules, null, false, true);

        // Assert
        result.IsError.Should().BeTrue();
        result.TopError.Should().Be(ApplicationSectionDefinitionErrors.DuplicateFieldKey);
    }

    [Fact]
    public void EvaluateRepeatRule_ShouldReturnZero_WhenSectionIsNotRepeatable()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 1, repeatRule: null).Value;

        // Act
        var result = section.EvaluateRepeatRule("3");

        // Assert
        result.Value.Should().Be(0);
    }

    [Fact]
    public void EvaluateRepeatRule_ShouldReturnCorrectCount_WhenRepeatRuleIsSet()
    {
        // Arrange
        var rule = RepeatRule.Create("depend_field", RepeatMode.ExactValue).Value;
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 1, rule).Value;

        // Act
        var result = section.EvaluateRepeatRule("5");

        // Assert
        result.Value.Should().Be(5);
    }

    [Fact]
    public void RemoveField_ShouldRemoveFromList_WhenFieldExists()
    {
        // Arrange
        var section = ApplicationSectionDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Section", 1).Value;
        var field = section.AddNewField("temp", FieldType.Text, _dummyRules, null, false, false).Value;

        // Act
        var result = section.RemoveField(field.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        section.Fields.Should().BeEmpty();
    }
}