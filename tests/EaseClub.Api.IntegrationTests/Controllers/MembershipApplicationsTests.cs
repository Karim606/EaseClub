using EaseClub.Api.IntegrationTests.Common;
using EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.UpdateAnswer;
using EaseClub.Application.Features.MembershipApplications.Queries;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace EaseClub.Api.IntegrationTests.Controllers
{
    public class MembershipApplicationsTests : BaseIntegrationTest
    {
        private readonly Guid _testClubId =
            Guid.Parse("9f3a8b6e-2a7d-4b5c-9d9c-1e8c4c2f7a31");
        private  readonly Guid SeedMembershipTypeId = Guid.Parse("a1e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d94");

        private  readonly Guid SeedMembershipPlanId = Guid.Parse("7c2d4a8e-1b9f-4e2a-8f3c-5b6d9a1e0c47");

        public MembershipApplicationsTests(
            ApiWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Create_Application_ReturnsCreated()
        {
            await LoginAsMemberAsync();

            // Arrange
            var (templateId, fieldId) = await CreateFullTemplateHierarchyAsync();


            var command = new CreateApplicationCommand(
                _testClubId,
                templateId,
                SeedMembershipTypeId,
                SeedMembershipPlanId
                );

            // Act
            var response = await Client.PostAsJsonAsync(
                "/api/v1/membership-applications",
                command);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var applicationId =
                await response.Content.ReadFromJsonAsync<Guid>();

            Assert.NotEqual(Guid.Empty, applicationId);
        }

        [Fact]
        public async Task Get_Application_ReturnsApplication()
        {
            await LoginAsMemberAsync();

            var applicationId = await CreateApplicationAsync();

            var response = await Client.GetAsync(
                $"/api/v1/membership-applications/{applicationId}");

            response.EnsureSuccessStatusCode();

            var result =  await response.Content
                           .ReadFromJsonAsync<ApplicationResponse>();

            Assert.NotNull(result);
            Assert.Equal(applicationId, result!.ApplicationId);
            Assert.NotNull(result.TemplateStructure);
            Assert.NotNull(result.Answers);
        }

        [Fact]
        public async Task SetNewAnswer_SetsSuccessfully()
        {
            await LoginAsMemberAsync();

            var (applicationId, fieldId) =
                await CreateApplicationWithFieldAsync();

            var setAnswerCommand = new SetAnswerCommand(
                applicationId,
                fieldId,
                "John Doe",
                0);

            var response = await Client.PatchAsJsonAsync(
                $"/api/v1/membership-applications/{applicationId}/answers",
                setAnswerCommand);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAnswer_WhenAnswerExists_UpdatesValue()
        {
            await LoginAsMemberAsync();

            // Arrange
            var (applicationId, fieldId) =
                await CreateApplicationWithFieldAsync();

            // 1️⃣ Insert initial answer
            await Client.PatchAsJsonAsync(
                $"/api/v1/membership-applications/{applicationId}/answers",
                new SetAnswerCommand(
                    applicationId,
                    fieldId,
                    "Old Value",
                    0));

            // 2️⃣ Update existing answer
            var updateResponse = await Client.PatchAsJsonAsync(
                $"/api/v1/membership-applications/{applicationId}/answers",
                new SetAnswerCommand(
                    applicationId,
                    fieldId,
                    "Updated Value",
                    0));

            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // 3️⃣ Get application again
            var getResponse = await Client.GetAsync(
                $"/api/v1/membership-applications/{applicationId}");

            getResponse.EnsureSuccessStatusCode();

            var result = await getResponse.Content
                .ReadFromJsonAsync<ApplicationResponse>();

            // 4️⃣ Assert answer was UPDATED not duplicated
            Assert.NotNull(result);
            Assert.Single(result!.Answers);

            var answer = result.Answers.First();

            Assert.Equal(fieldId, answer.FieldDefinitionId);
            Assert.Equal("Updated Value", answer.Value);
            Assert.Equal(0, answer.InstanceIndex);
        }

        [Fact]
        public async Task RemoveAnswer_RemovesSuccessfully()
        {
            await LoginAsMemberAsync();

            var (applicationId, fieldId) =
                await CreateApplicationWithFieldAsync();

            // First update to insert answer
            await Client.PatchAsJsonAsync(
                $"/api/v1/membership-applications/{applicationId}/answers",
                new SetAnswerCommand(
                    applicationId,
                    fieldId,
                    "To Be Removed",
                    0));

            // Act
            var response = await Client.DeleteAsync(
                $"/api/v1/membership-applications/{applicationId}/answers/{fieldId}?index=0");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #region Helpers

        private async Task<Guid> CreateApplicationAsync()
        {
            var (templateId, _) =
                await CreateFullTemplateHierarchyAsync();

            var command = new CreateApplicationCommand(
                _testClubId,
                templateId,
                SeedMembershipTypeId,
                SeedMembershipPlanId
                );

            var response = await Client.PostAsJsonAsync(
                "/api/v1/membership-applications",
                command);

            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        private async Task<(Guid applicationId, Guid fieldId)>
            CreateApplicationWithFieldAsync()
        {
            var (templateId, fieldId) =
                await CreateFullTemplateHierarchyAsync();

            var command = new CreateApplicationCommand(
                _testClubId,
                templateId,
                SeedMembershipTypeId,
                SeedMembershipPlanId
                );

            var response = await Client.PostAsJsonAsync(
                "/api/v1/membership-applications",
                command);

            var applicationId =
                await response.Content.ReadFromJsonAsync<Guid>();

            return (applicationId, fieldId);
        }

        private async Task<(Guid templateId, Guid fieldId)>
            CreateFullTemplateHierarchyAsync()
        {
            await LoginAsAdminAsync();

            // 1️⃣ Create Template
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), _testClubId, "FullTemplate").Value;
            await  DbContext.ApplicationTemplateDefinitions .AddAsync(template);
            
            await DbContext.SaveChangesAsync();

            //2 AddStep
            var step = template.AddNewStep("PI", "FirstStep", 1).Value;
            await DbContext.ApplicationStepDefinitions.AddAsync(step);

            await DbContext.SaveChangesAsync();

            // 3️⃣ Insert Section
            var section = step.AddNewSection("Details", 1, null).Value;
            await DbContext.ApplicationSectionDefinitions.AddAsync(section);

            await DbContext.SaveChangesAsync();

            // 4️⃣ Insert Field
            var rules = ValidationRuleSet
                .Create(true, 1, 100, null, null, null).Value;

            var field = section.AddNewField(
                "FullName",
                FieldType.Text,
                rules,
                null,
                false,
                1).Value;
            await DbContext.ApplicationFieldDefinitions.AddAsync(field);

            await DbContext.SaveChangesAsync();

            return (template.Id, field.Id);
        }


        #endregion
    }
}
