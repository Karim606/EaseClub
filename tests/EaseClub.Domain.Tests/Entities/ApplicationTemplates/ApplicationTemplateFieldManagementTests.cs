using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.ApplicationTemplates
{
    public class ApplicationTemplateFieldManagementTests
    {
        private readonly ValidationRuleSet _dummyRules = ValidationRuleSet.Create(false).Value;

        private ApplicationTemplateDefinition CreateTemplateWithSection(out Guid sectionId)
        {
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Club Template").Value;
            var step = template.AddNewStep("General", "Personal Info", 0).Value;
            var section = step.AddNewSection("Contact Details", 0).Value;
            sectionId = section.Id;
            return template;
        }

        [Fact]
        public void AddFieldToSection_ShouldSucceed_WhenKeyIsUniqueAcrossTemplate()
        {
            // Arrange
            var template = CreateTemplateWithSection(out var sectionId);

            // Act
            var result = template.AddFieldToSection(
                Guid.NewGuid(), sectionId, "email_address", "Email",
                FieldType.Text, _dummyRules, null, true);

            // Assert
            result.IsSuccess.Should().BeTrue();
            template.Steps[0].Sections[0].Fields.Should().HaveCount(1);
            result.Value.Key.Should().Be("email_address");
        }

        [Fact]
        public void AddFieldToSection_ShouldReturnConflict_WhenKeyAlreadyExistsInDifferentSection()
        {
            // Arrange
            var template = CreateTemplateWithSection(out var section1Id);

            // Add second section
            var step = template.Steps[0];
            var section2 = step.AddNewSection("Emergency Contact", 1).Value;

            // Add field to section 1
            template.AddFieldToSection(Guid.NewGuid(), section1Id, "phone", "Phone", FieldType.Text, _dummyRules, null, false);

            // Act - Try to add same key to section 2
            var result = template.AddFieldToSection(Guid.NewGuid(), section2.Id, "phone", "Other Phone", FieldType.Text, _dummyRules, null, false);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Conflict);
        }

        [Fact]
        public void RemoveField_ShouldUpdateSectionAndShiftOrders()
        {
            // Arrange
            var template = CreateTemplateWithSection(out var sectionId);
            var f1 = template.AddFieldToSection(Guid.NewGuid(), sectionId, "f1", "L1", FieldType.Text, _dummyRules, null, false).Value;
            var f2 = template.AddFieldToSection(Guid.NewGuid(), sectionId, "f2", "L2", FieldType.Text, _dummyRules, null, false).Value;

            // Act
            template.RemoveField(sectionId, f1.Id);

            // Assert
            var section = template.Steps[0].Sections[0];
            section.Fields.Should().HaveCount(1);
            section.Fields[0].Id.Should().Be(f2.Id);
            section.Fields[0].Order.Should().Be(1); // Shifted down from 2
        }

        [Fact]
        public void ReorderFieldsInSection_ShouldUpdateAllOrders_WhenValidListProvided()
        {
            // 1. Arrange: Create a template with one section and three fields
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Club Template").Value;
            var step = template.AddNewStep("General", "Personal Info", 0).Value;
            var section = step.AddNewSection("Contact Details", 0).Value;
            var sectionId = section.Id;

            var f1 = template.AddFieldToSection(Guid.NewGuid(), sectionId, "f1", "Field 1", FieldType.Text, _dummyRules, null, false).Value;
            var f2 = template.AddFieldToSection(Guid.NewGuid(), sectionId, "f2", "Field 2", FieldType.Text, _dummyRules, null, false).Value;
            var f3 = template.AddFieldToSection(Guid.NewGuid(), sectionId, "f3", "Field 3", FieldType.Text, _dummyRules, null, false).Value;

            // Current Orders: f1=1, f2=2, f3=3 (based on your Count+1 logic)

            // 2. Act: We want to move f3 to the top: [f3, f1, f2]
            var newOrderIds = new List<Guid> { f3.Id, f1.Id, f2.Id };
            var result = template.ReorderFieldsInSection(sectionId, newOrderIds);

            // 3. Assert
            result.IsSuccess.Should().BeTrue();

            // Verify f3 is now 1st
            section.Fields.First(f => f.Id == f3.Id).Order.Should().Be(1);

            // Verify f1 is now 2nd
            section.Fields.First(f => f.Id == f1.Id).Order.Should().Be(2);

            // Verify f2 is now 3rd
            section.Fields.First(f => f.Id == f2.Id).Order.Should().Be(3);
        }

        [Fact]
        public void ReorderFieldsInSection_ShouldReturnError_WhenIdCountMismatches()
        {
            // Arrange
            var template = CreateTemplateWithSection(out var sectionId);
            template.AddFieldToSection(Guid.NewGuid(), sectionId, "f1", "L1", FieldType.Text, _dummyRules, null, false);
            template.AddFieldToSection(Guid.NewGuid(), sectionId, "f2", "L2", FieldType.Text, _dummyRules, null, false);

            // Act: Send only 1 ID instead of 2
            var result = template.ReorderFieldsInSection(sectionId, new List<Guid> { Guid.NewGuid() });

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Contain("Count mismatch");
        }
    }
}
