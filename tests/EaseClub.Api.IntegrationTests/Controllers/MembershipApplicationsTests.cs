using EaseClub.Api.IntegrationTests.Common;
using EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep;
using EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.UpdateAnswer;
using EaseClub.Application.Features.MembershipApplications.Queries;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EaseClub.Api.IntegrationTests.Controllers
{
    public class MembershipApplicationsTests : BaseIntegrationTest
    {
        private readonly Guid _testClubId = Guid.Parse("9f3a8b6e-2a7d-4b5c-9d9c-1e8c4c2f7a31");
        private readonly Guid SeedMembershipTypeId = Guid.Parse("a1e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d94");
        private readonly Guid SeedMembershipPlanId = Guid.Parse("7c2d4a8e-1b9f-4e2a-8f3c-5b6d9a1e0c47");

        public MembershipApplicationsTests(ApiWebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task Create_Application_ReturnsCreated()
        {
            await LoginAsMemberAsync();

            // Arrange: Use refactored helper
            var (templateId, _, _, _) = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);
            var command = new CreateApplicationCommand(_testClubId, templateId, SeedMembershipTypeId, SeedMembershipPlanId);

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/membership-applications", command);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var applicationId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, applicationId);
        }

        [Fact]
        public async Task Get_Application_ReturnsApplication()
        {
            await LoginAsMemberAsync();
            var (applicationId, _) = await SetupApplicationWithTemplateAsync();
         //   ClearTracker();
            // Act
            var response = await Client.GetAsync($"/api/v1/membership-applications/{applicationId}");

            // Assert
            response.EnsureSuccessStatusCode();


            var result = await response.Content.ReadFromJsonAsync<ApplicationResponse>(JsonOptions);
            Assert.NotNull(result);
            Assert.Equal(applicationId, result!.Id);
            Assert.NotNull(result.Template);
        }

        [Fact]
        public async Task CompleteStep_ReturnsSuccess_WhenAnswersAreValid()
        {
            await LoginAsMemberAsync();
            var (appId, fieldId) = await SetupApplicationWithTemplateAsync();

            // Prepare answers for the step
            var answers = new List<AnswerDto>
            {
                new(fieldId, "full_name", "John Wick", 0)
            };

            var command = new CompleteStepCommand(appId, 1, answers);
           // ClearTracker();
            // Act
            var response = await Client.PostAsJsonAsync($"/api/v1/membership-applications/{appId}/steps/{command.StepOrder}/" +
                $"complete",answers);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify answers were persisted
            var app = await GetApplicationAsync(appId);
            Assert.Contains(app.Answers, a => a.Value == "John Wick");
        }

        [Fact]
        public async Task Submit_ReturnsSuccess_WhenAllStepsCompleted()
        {
            await LoginAsMemberAsync();
            var (appId, fieldId) = await SetupApplicationWithTemplateAsync();

            // 1. Complete the step first (Transitioning from Draft)
            await CompleteStepAsync(appId, fieldId, "Valid Answer");
           // ClearTracker();
            // 2. Act: Submit
            var response = await Client.PostAsJsonAsync($"/api/v1/membership-applications/{appId}/submit", new { });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // 3. Verify status in DB
            var updatedApp = await DbContext.MembershipApplications.FindAsync(appId);
            // Assuming Status 1 = Submitted/PendingApproval
            Assert.NotEqual(0, (int)updatedApp!.Status);
        }


        //[Fact]
        //public async Task SetNewAnswer_SetsSuccessfully()
        //{
        //    await LoginAsMemberAsync();
        //    var (applicationId, fieldId) = await SetupApplicationWithTemplateAsync();

        //    var setAnswerCommand = new SetAnswerCommand(applicationId, fieldId, "John Doe", 0);

        //    // Act
        //    var response = await Client.PatchAsJsonAsync($"/api/v1/membership-applications/{applicationId}/answers", setAnswerCommand);

        //    // Assert
        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //}

        //[Fact]
        //public async Task UpdateAnswer_WhenAnswerExists_UpdatesValue()
        //{
        //    await LoginAsMemberAsync();
        //    var (applicationId, fieldId) = await SetupApplicationWithTemplateAsync();

        //    // 1. Insert initial answer
        //    await Client.PatchAsJsonAsync($"/api/v1/membership-applications/{applicationId}/answers",
        //        new SetAnswerCommand(applicationId, fieldId, "Old Value", 0));

        //    // 2. Update existing answer
        //    var updateResponse = await Client.PatchAsJsonAsync($"/api/v1/membership-applications/{applicationId}/answers",
        //        new SetAnswerCommand(applicationId, fieldId, "Updated Value", 0));

        //    Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        //    // 3. Verify
        //    var getResponse = await Client.GetAsync($"/api/v1/membership-applications/{applicationId}");
        //    var result = await getResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        //    Assert.Single(result!.Answers);
        //    Assert.Equal("Updated Value", result.Answers.First().Value);
        //}

        //[Fact]
        //public async Task RemoveAnswer_RemovesSuccessfully()
        //{
        //    await LoginAsMemberAsync();
        //    var (applicationId, fieldId) = await SetupApplicationWithTemplateAsync();

        //    // Arrange: Seed an answer to delete
        //    await Client.PatchAsJsonAsync($"/api/v1/membership-applications/{applicationId}/answers",
        //        new SetAnswerCommand(applicationId, fieldId, "Delete Me", 0));

        //    // Act
        //    var response = await Client.DeleteAsync($"/api/v1/membership-applications/{applicationId}/answers/{fieldId}?index=0");

        //    // Assert
        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //}

        #region Private Test Orchestrators

        /// <summary>
        /// Combines Helper Template creation and API Application creation to provide a ready-to-test state.
        /// </summary>
        private async Task<(Guid applicationId, Guid fieldId)> SetupApplicationWithTemplateAsync()
        {
            // We need to be Admin to seed the Template, then switch back to Member if necessary
            // However, since we write directly to DbContext in Helpers, we don't actually need to LoginAsAdmin here.
            var (templateId, _, _, fieldId) = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var command = new CreateApplicationCommand(_testClubId, templateId, SeedMembershipTypeId, SeedMembershipPlanId);
            var response = await Client.PostAsJsonAsync("/api/v1/membership-applications", command);

            var applicationId = await response.Content.ReadFromJsonAsync<Guid>();
            return (applicationId, fieldId);
        }

        private async Task CompleteStepAsync(Guid appId, Guid fieldId, string value)
        {
            var answers = new List<AnswerDto> { new(fieldId, "full_name", value, 0) };
            var command = new CompleteStepCommand(appId, 1, answers);

            var response = await Client.PostAsJsonAsync($"/api/v1/membership-applications/{appId}/steps/{command.StepOrder}/" +
                $"complete", answers);
            
        }

        private async Task<ApplicationResponse> GetApplicationAsync(Guid appId)
        {
            return (await Client.GetFromJsonAsync<ApplicationResponse>($"/api/v1/membership-applications/{appId}",JsonOptions))!;
        }
        #endregion
    }
}
