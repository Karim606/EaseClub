using EaseClub.Api.IntegrationTests.Common;
using EaseClub.Application.Features.MembershipApplications;
using EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep;
using EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplication;
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
        private readonly Guid SeedMembershipTypeId = Guid.Parse("b2e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d96");
        private readonly Guid SeedMembershipPlanId = Guid.Parse("c3e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d97");

        public MembershipApplicationsTests(ApiWebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task Create_Application_ReturnsCreated()
        {
            await LoginAsMemberAsync();

            // Create command with correct parameter order
            var command = new CreateApplicationCommand(_testClubId, SeedMembershipTypeId, SeedMembershipPlanId, null);

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


            var result = await response.Content.ReadFromJsonAsync<ApplicationUserResponse>(JsonOptions);
            Assert.NotNull(result);
            Assert.Equal(applicationId, result!.Id);
            Assert.NotEmpty(result.Steps);
        }

        [Fact]
        public async Task CompleteStep_ReturnsSuccess_WhenAnswersAreValid()
        {
            await LoginAsMemberAsync();
            var (appId, fieldId) = await SetupApplicationWithTemplateAsync();

            // Prepare answers for the step
            var answers = new List<AnswerRequestDto>
            {
                new(fieldId, "John Wick", null)
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
            var fieldValue = app.Steps
                .SelectMany(s => s.Sections)
                .SelectMany(sec => sec.Fields)
                .FirstOrDefault(f => f.Id == fieldId)?.Value;
            Assert.Equal("John Wick", fieldValue);
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


        #region Private Test Orchestrators

        private async Task<(Guid applicationId, Guid fieldId)> SetupApplicationWithTemplateAsync()
        {
            var plan = await DbContext.MembershipPlans.FirstAsync(p => p.Id == SeedMembershipPlanId);
            var template = await DbContext.ApplicationTemplateDefinitions.FirstAsync(t => t.Id == plan.ApplicationTemplateId!.Value);
            var fieldId = template.Steps.First().Sections.First().Fields.First().Id;

            var command = new CreateApplicationCommand(_testClubId, SeedMembershipTypeId, SeedMembershipPlanId, null);
            var response = await Client.PostAsJsonAsync("/api/v1/membership-applications", command);

            var applicationId = await response.Content.ReadFromJsonAsync<Guid>();
            return (applicationId, fieldId);
        }

        private async Task CompleteStepAsync(Guid appId, Guid fieldId, string value)
        {
            var answers = new List<AnswerRequestDto> { new(fieldId, value, null) };
            var command = new CompleteStepCommand(appId, 1, answers);

            var response = await Client.PostAsJsonAsync($"/api/v1/membership-applications/{appId}/steps/{command.StepOrder}/" +
                $"complete", answers);
            
        }

        private async Task<ApplicationUserResponse> GetApplicationAsync(Guid appId)
        {
            return (await Client.GetFromJsonAsync<ApplicationUserResponse>($"/api/v1/membership-applications/{appId}",JsonOptions))!;
        }
        #endregion
    }
}
