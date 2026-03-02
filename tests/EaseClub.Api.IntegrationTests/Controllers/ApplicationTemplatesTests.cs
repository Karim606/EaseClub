using BetterStack.Logs;
using EaseClub.Api.IntegrationTests.Common;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Field;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Field.AddField;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Field.UpdateField;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Section.AddSection;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Section.UpdateSection;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Step.AddStep;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Step.UpdateStep;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.CreateTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpdateTemplate;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Clubs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Api.IntegrationTests.Controllers
{
    public class ApplicationTemplatesTests : BaseIntegrationTest
    {
        // Use the Seeded ClubId from your DbInitializer
        private readonly Guid _testClubId = Guid.Parse("9f3a8b6e-2a7d-4b5c-9d9c-1e8c4c2f7a31");

        public ApplicationTemplatesTests(ApiWebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task Full_Aggregate_Hierarchy_Setup_AddField()
        {
            await LoginAsAdminAsync();

            // 1. Create Template
            var templateId = await CreateTemplateAsync("Full Form");

            // 2. Add Step
            var stepCmd = new AddStepCommand(_testClubId, "CategoryA", "Step 1", 1) { TemplateId = templateId };
            var stepRes = await Client.PostAsJsonAsync($"/api/v1/admin/application-templates/{templateId}/steps",
                stepCmd);
            var stepId = await stepRes.Content.ReadFromJsonAsync<Guid>();

            // 3. Add Section
            var sectionCmd = new AddSectionCommand(_testClubId, "Details", 1, null);
            var secRes = await Client.PostAsJsonAsync($"/api/v1/admin/application-templates/steps/{stepId}/sections",
                sectionCmd);
            var sectionId = await secRes.Content.ReadFromJsonAsync<Guid>();
            Assert.Equal(HttpStatusCode.OK, secRes.StatusCode);

            // 4. Add Field
            var fieldCmd = new AddFieldCommand(_testClubId,templateId, "Field 1", "label", false, FieldType.Text,
                new ValidationRuleSetDto(), null) {TemplateId =templateId};

          var fieldRes = await Client.PostAsJsonAsync($"/api/v1/admin/application-templates/sections/{sectionId}/fields",
                fieldCmd);

            Assert.Equal(HttpStatusCode.OK, fieldRes.StatusCode);
            var fieldId = await fieldRes.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, fieldId);
        }


        [Fact]
        public async Task GetTemplates_ReturnsPaginatedResults()
        {
            // Arrange
            await LoginAsAdminAsync();
            // Construct query string for pagination and clubId
            var url = $"/api/v1/admin/application-templates?clubId={_testClubId}&Page=1&Limit=10";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("items", result); // Basic check for paginated structure
        }

        #region Update Methods
        [Fact]
        public async Task UpdateTemplate_ReturnsNoContent_WhenOwnershipIsValid()
        {
            // Arrange
            await LoginAsAdminAsync();

            // 1. First create a template to update
            var createCmd = new CreateTemplateCommand(_testClubId, "Initial Name");
            var createResponse = await Client.PostAsJsonAsync("/api/v1/admin/application-templates", createCmd);
            var templateId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // 2. Prepare update
            var updateCmd = new UpdateTemplateCommand(_testClubId, "Updated Name");

            // Act
            var response = await Client.PutAsJsonAsync($"/api/v1/admin/application-templates/{templateId}", updateCmd);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateStep_Endpoint_UpdatesStep()
        {
            await LoginAsAdminAsync();

            // Arrange
           var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var newTitle = "Updated Step";

            var updateCmd = new
            {
                ClubId = _testClubId,
                Category = "CatA",
                Title = newTitle,
            };

            // Act
            var response = await Client.PutAsJsonAsync(
                $"/api/v1/admin/application-templates/steps/{ids.stepId}",
                updateCmd);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Assert
            ClearTracker();

            var updatedStep = await DbContext.ApplicationStepDefinitions
                .FirstAsync(x => x.Id == ids.stepId);

            Assert.Equal(newTitle, updatedStep.Title);
        }

        [Fact]
        public async Task UpdateSection_Endpoint_UpdatesSection()
        {
            await LoginAsAdminAsync();

            // 1. Use DbContext to get an existing Step and Section
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            // 2. Prepare the update command
            var updateSectionCmd = new
            {
                ClubId = _testClubId,
                Title = "Updated Section Title",
            };

            // 3. Call your API endpoint
            var response = await Client.PutAsJsonAsync(
                $"/api/v1/admin/application-templates/sections/{ids.sectionId}",
                updateSectionCmd
            );

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // 4. Verify via DbContext
            ClearTracker(); 

            var updatedSection = await DbContext.ApplicationSectionDefinitions.FindAsync(ids.sectionId);
            Assert.Equal("Updated Section Title", updatedSection!.Title);
        }
        [Fact]
        public async Task UpdateField_ShouldSucceed_WhenValidDataProvided()
        {
            await LoginAsAdminAsync();
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var updateCmd = new UpdateFieldCommand(_testClubId, "updated_label", true,
                new ValidationRuleSetDto { IsRequired = true },
                null, null);
            //{
            //    ClubId = _testClubId,
            //    Key = "updated_key",
            //    PersistToMembership = true,
            //    Type = FieldType.Text,
            //    ValidationRules = new ValidationRuleSetDto { IsRequired = true }
                
            //};

            var response = await Client.PutAsJsonAsync($"/api/v1/admin/application-templates/fields/{ids.fieldId}", updateCmd);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify state
            ClearTracker();

            var field = await DbContext.ApplicationFieldDefinitions.FindAsync(ids.fieldId);
            Assert.Equal("updated_label", field!.Label);
        }
        //[Fact]
        //public async Task UpdateField_Endpoint_UpdatesField()
        //{
        //    await LoginAsAdminAsync();

        //    // ----------------------------
        //    // 1️⃣ Create a template, step, section, and field
        //    // ----------------------------
        //    var templateId = await CreateTemplateAsync("Field Update Template");

        //    var stepId = await InsertStepAsync(templateId, "StepCat", "Step1", 1);
        //    var sectionId = await InsertSectionAsync(stepId, "Section1", 1);

        //    var fieldId = await InsertFieldAsync(
        //        sectionId,
        //        key: "Field1",
        //        order: 1,
        //        type: FieldType.Text,
        //        rules: ValidationRuleSet.Create(true, 1, 10, null, null, null).Value,
        //        visibility: null,
        //        persistToMembership: false
        //    );

        //    // ----------------------------
        //    // 2️⃣ Prepare update payload
        //    // ----------------------------
        //    var newTitle = "Updated Field Key";
        //    var updateFieldCmd = new
        //    {
        //        ClubId = _testClubId,
        //        Key = newTitle,
        //        PersistToMembership = true,
        //        Type = FieldType.Text,
        //        ValidationRules = ValidationRuleSet.Create(true, 1, 10, null, null, null).Value,
        //    };

        //    // ----------------------------
        //    // 3️⃣ Call UpdateField API
        //    // ----------------------------
        //    var response = await Client.PutAsJsonAsync(
        //        $"/api/v1/admin/application-templates/fields/{fieldId}",
        //        updateFieldCmd
        //    );

        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        //    // ----------------------------
        //    // 4️⃣ Verify via DbContext
        //    // ----------------------------
        //    // Detach tracked entities to avoid stale data
        //    DbContext.ChangeTracker.Clear();

        //    var updatedField = await DbContext.ApplicationFieldDefinitions
        //        .FirstAsync(f => f.Id == fieldId);

        //    Assert.Equal(newTitle, updatedField.Key);
        //    Assert.True(updatedField.PersistToMembership);
        //}
        #endregion


        [Fact]
        #region Delete Methods
        public async Task DeleteTemplate_Requires_ClubId_In_Header()
        {
            // Arrange
            await LoginAsAdminAsync();

            // Create one to delete
            var createCmd = new CreateTemplateCommand(_testClubId, "To Be Deleted");
            var createRes = await Client.PostAsJsonAsync("/api/v1/admin/application-templates", createCmd);
            var templateId = await createRes.Content.ReadFromJsonAsync<Guid>();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/application-templates/{templateId}");
            request.Headers.Add("X-Club-Id", _testClubId.ToString()); // Required by your controller

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }


        [Fact]
        public async Task RemoveStep_ShouldDeleteStepAndReorderRemainingSteps()
        {
            // Arrange
            await LoginAsAdminAsync();
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);
  
            // Add 3 steps: Order 0, 1, 2
            var step2Id = await _helpers.AddStepAsync(ids.templateId, "Step 2", 1);
            var step3Id = await _helpers.AddStepAsync(ids.templateId, "Step 3", 2);
            var step4Id = await _helpers.AddStepAsync(ids.templateId, "Step 4", 3);

            // Act: Create Request manually to add the Header
            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"/api/v1/admin/application-templates/steps/{ids.stepId}?templateId={ids.templateId}");

            // Adding the missing Club Header
            request.Headers.Add("X-Club-Id", _testClubId.ToString());

            var response = await Client.SendAsync(request);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            ClearTracker();
            // Verify DB State
            var template = await DbContext.ApplicationTemplateDefinitions.Include(t => t.Steps)
                .FirstOrDefaultAsync(t => t.Id == ids.templateId);
            template!.Steps.Should().HaveCount(3);
            template.Steps.Should().NotContain(s => s.Id == ids.stepId);

            // Check Reordering: Step 3 should now be at Order 1 (previously 2)
            var step3 = template.Steps.First(s => s.Id == step2Id);
            step3.Order.Should().Be(0);
        }

        [Fact]
        public async Task RemoveSection_ShouldSucceedAndShiftOrders()
        {
            // Arrange
            await LoginAsAdminAsync();
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var sec1Id = await _helpers.AddSectionAsync(ids.stepId, "Section 1", 1);
            var sec2Id = await _helpers.AddSectionAsync(ids.stepId, "Section 2", 2);

            // Act
            ClearTracker();
            var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/admin/application-templates/steps/{ids.stepId}/sections/{ids.sectionId}");

            request.Headers.Add("X-Club-Id", _testClubId.ToString());

            var response = await Client.SendAsync(request);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var step = await DbContext.ApplicationStepDefinitions.Include(st =>st.Sections)
                .FirstOrDefaultAsync(st=>st.Id ==ids.stepId);  

            step.Sections.Should().HaveCount(2);
            step.Sections.First().Id.Should().Be(sec1Id);
            step.Sections.First().Order.Should().Be(0); // Shifted from 1 to 0
        }

        [Fact]
        public async Task RemoveField_ShouldRemoveFieldFromSection()
        {
            // Arrange
            await LoginAsAdminAsync();
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);
            var fieldId = await _helpers.AddFieldAsync(ids.templateId, ids.sectionId, "phone_number", "Phone");

            // Act

            var request = new HttpRequestMessage(HttpMethod.Delete,
            $"/api/v1/admin/application-templates/sections/{ids.sectionId}/fields/{fieldId}?templateId={ids.templateId}");

            request.Headers.Add("X-Club-Id", _testClubId.ToString());

            var response = await Client.SendAsync(request);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            ClearTracker();
            // Verify via API (using our global JsonOptions)
            var templateResponse =  await DbContext.ApplicationTemplateDefinitions.Include(t => t.Steps)
                .ThenInclude(st=>st.Sections)
                .ThenInclude(s => s.Fields)
                .FirstOrDefaultAsync(t => t.Id == ids.templateId);
            var section = templateResponse.Steps.SelectMany(s => s.Sections).First(s => s.Id == ids.sectionId);

            section.Fields.Should().NotContain(f => f.Id == fieldId);
        }

        [Fact]
        public async Task RemoveStep_FromDifferentClub_ShouldReturnForbidden()
        {
            // Arrange: Seed two different clubs
            await LoginAsAdminAsync(); // Logged in as Admin of Club A
            var otherClubId = Guid.NewGuid();
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(otherClubId);
            var otherStepId = await _helpers.AddStepAsync(ids.templateId, "Enemy Step", 1);

            // Act
            var response = await Client.DeleteAsync(
                $"/api/v1/admin/application-templates/steps/{otherStepId}?templateId={ids.templateId}");

            // Assert: Ownership validation should trigger
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion


        [Fact]
        public async Task Unauthorized_User_Cannot_Access_Templates()
        {
            // Arrange - Do NOT call Login
            var command = new CreateTemplateCommand(_testClubId, "Secret Template");

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/admin/application-templates", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #region Helpers
        private async Task<Guid> CreateTemplateAsync(string name)
        {
            var res = await Client.PostAsJsonAsync("/api/v1/admin/application-templates", new CreateTemplateCommand(_testClubId, name));
            return await res.Content.ReadFromJsonAsync<Guid>();
        }
        #endregion
    }
}
