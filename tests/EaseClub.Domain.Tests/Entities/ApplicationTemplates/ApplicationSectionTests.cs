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