using BetterStack.Logs;
using EaseClub.Api.IntegrationTests.Common;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Field.AddField;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Section.AddSection;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Section.UpdateSection;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Step.AddStep;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.CreateTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpdateTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Field;
using EaseClub.Domain.ApplicationTemplates;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Step.UpdateStep;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Field.UpdateField;

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
            var stepRes = await Client.PostAsJsonAsync($"/api/v1/admin/application-templates/{templateId}/steps", stepCmd);
            var stepId = await stepRes.Content.ReadFromJsonAsync<Guid>();

            // 3. Add Section
            var sectionCmd = new AddSectionCommand(_testClubId, "Details", 1, null);
            var secRes = await Client.PostAsJsonAsync($"/api/v1/admin/application-templates/steps/{stepId}/sections", sectionCmd);
            var sectionId = await secRes.Content.ReadFromJsonAsync<Guid>();
            Assert.Equal(HttpStatusCode.OK, secRes.StatusCode);

            // 4. Add Field
            var fieldCmd = new AddFieldCommand(_testClubId, "Field 1", 1,false,FieldType.Text,new ValidationRuleSetDto(),null);
            var fieldRes = await Client.PostAsJsonAsync($"/api/v1/admin/application-templates/sections/{sectionId}/fields", fieldCmd);

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
            var templateId = await CreateTemplateAsync("Full");
            var stepId = await InsertStepAsync(templateId, "Cat1", "Original Step", 1);

            var newTitle = "Updated Step";

            var updateCmd = new
            {
                ClubId = _testClubId,
                Category = "CatA",
                Title = newTitle,
            };

            // Act
            var response = await Client.PutAsJsonAsync(
                $"/api/v1/admin/application-templates/steps/{stepId}",
                updateCmd);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Assert
            DbContext.ChangeTracker.Clear();

            var updatedStep = await DbContext.ApplicationStepDefinitions
                .FirstAsync(x => x.Id == stepId);

            Assert.Equal(newTitle, updatedStep.Title);
        }

        [Fact]
        public async Task UpdateSection_Endpoint_UpdatesSection()
        {
            await LoginAsAdminAsync();

            // 1. Use DbContext to get an existing Step and Section
            var templateId = await CreateTemplateAsync("Full");
            var stepId = await InsertStepAsync(templateId, "Cat1", "Original Step", 1);
            var sectionId = await InsertSectionAsync(stepId);

            // 2. Prepare the update command
            var updateSectionCmd = new
            {
                ClubId = _testClubId,
                Title = "Updated Section Title",
            };

            // 3. Call your API endpoint
            var response = await Client.PutAsJsonAsync(
                $"/api/v1/admin/application-templates/sections/{sectionId}",
                updateSectionCmd
            );

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // 4. Verify via DbContext
            DbContext.ChangeTracker.Clear();
            var updatedSection = await DbContext.ApplicationSectionDefinitions.FindAsync(sectionId);
            Assert.Equal("Updated Section Title", updatedSection!.Title);
        }

        [Fact]
        public async Task UpdateField_Endpoint_UpdatesField()
        {
            await LoginAsAdminAsync();

            // ----------------------------
            // 1️⃣ Create a template, step, section, and field
            // ----------------------------
            var templateId = await CreateTemplateAsync("Field Update Template");

            var stepId = await InsertStepAsync(templateId, "StepCat", "Step1", 1);
            var sectionId = await InsertSectionAsync(stepId, "Section1", 1);

            var fieldId = await InsertFieldAsync(
                sectionId,
                key: "Field1",
                order: 1,
                type: FieldType.Text,
                rules: ValidationRuleSet.Create(true, 1, 10, null, null, null).Value,
                visibility: null,
                persistToMembership: false
            );

            // ----------------------------
            // 2️⃣ Prepare update payload
            // ----------------------------
            var newTitle = "Updated Field Key";
            var updateFieldCmd = new
            {
                ClubId = _testClubId,
                Key = newTitle,
                PersistToMembership = true,
                Type = FieldType.Text,
                ValidationRules = ValidationRuleSet.Create(true, 1, 10, null, null, null).Value,
            };

            // ----------------------------
            // 3️⃣ Call UpdateField API
            // ----------------------------
            var response = await Client.PutAsJsonAsync(
                $"/api/v1/admin/application-templates/fields/{fieldId}",
                updateFieldCmd
            );

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // ----------------------------
            // 4️⃣ Verify via DbContext
            // ----------------------------
            // Detach tracked entities to avoid stale data
            DbContext.ChangeTracker.Clear();

            var updatedField = await DbContext.ApplicationFieldDefinitions
                .FirstAsync(f => f.Id == fieldId);

            Assert.Equal(newTitle, updatedField.Key);
            Assert.True(updatedField.PersistToMembership);
        }
    #endregion

       
        [Fact]
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

        private async Task<Guid> InsertStepAsync(
                Guid templateId,
                string category = "CategoryA",
                string title = "Step 1",
                int order = 1)
        {
            var template = await DbContext.ApplicationTemplateDefinitions
                .Include(t => t.Steps)
                .FirstAsync(t => t.Id == templateId);

            var newStep = template.AddNewStep(category, title, order-1);

            await DbContext.ApplicationStepDefinitions.AddAsync(newStep.Value);

            await DbContext.SaveChangesAsync();

            return newStep.Value.Id;
        }

        private async Task<Guid> InsertSectionAsync(
            Guid stepId,
            string name = "Section 1",
            int order = 1,
            RepeatRule? rule = null)
        {
            var step = await DbContext.ApplicationStepDefinitions
                .Include(s => s.Sections)
                .FirstAsync(s => s.Id == stepId);

            var newSection =   step.AddNewSection(name, order-1, rule);

            await DbContext.ApplicationSectionDefinitions.AddAsync(newSection.Value);
            await DbContext.SaveChangesAsync();

            return newSection.Value.Id;
        }

        private async Task<Guid> InsertFieldAsync(
        Guid sectionId,
        string key = "Field1",
        int order = 1,
        FieldType type = FieldType.Text,
        ValidationRuleSet? rules = null,
        ConditionExpression? visibility = null,
        bool persistToMembership = false)
        {
            var section = await DbContext.ApplicationSectionDefinitions
                .Include(s => s.Fields)
                .FirstAsync(s => s.Id == sectionId);

            var isRequired = rules != null ? rules.IsRequired : true ;

            var validationRule = ValidationRuleSet.Create(isRequired
                ,rules?.MinLength
                ,rules?.MaxLength
                ,rules?.Regex
                ,rules?.MinValue
                ,rules?.MaxValue);

            var newField = section.AddNewField(key, type, validationRule.Value, visibility, persistToMembership, order-1);

            await  DbContext.ApplicationFieldDefinitions.AddAsync(newField.Value);
            await DbContext.SaveChangesAsync();

            return newField.Value.Id;
        }

        #endregion
    }
}
