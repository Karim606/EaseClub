using BetterStack.Logs;
using EaseClub.Api.IntegrationTests.Common;
using EaseClub.Application.Features.ApplicationTemplates;
using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.CreateTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.SyncMembershipTypes;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpdateTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate;
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
        public async Task UpsertTemplate_ShouldCreateTemplate_WhenTemplateIdIsNull()
        {
            await LoginAsAdminAsync();

            var command = new UpsertTemplateCommand(
                _testClubId,
                null,
                "New Template",
                new List<StepDetailsDto>()
            );

            var response = await Client.PostAsJsonAsync(
                "/api/v1/admin/application-templates/upsert",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            ClearTracker();

            var template = await DbContext.ApplicationTemplateDefinitions
                .FirstOrDefaultAsync(t => t.Name == "New Template");

            template.Should().NotBeNull();
            template!.ClubId.Should().Be(_testClubId);
        }

        [Fact]
        public async Task UpsertTemplate_ShouldUpdateTemplate_WhenTemplateExists()
        {
            await LoginAsAdminAsync();

            var ids = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var command = new UpsertTemplateCommand(
                _testClubId,
                ids.templateId,
                "Updated Template Name",
                new List<StepDetailsDto>()
            );

            var response = await Client.PostAsJsonAsync(
                "/api/v1/admin/application-templates/upsert",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            ClearTracker();

            var template = await DbContext.ApplicationTemplateDefinitions
                .FirstAsync(t => t.Id == ids.templateId);

            template.Name.Should().Be("Updated Template Name");
        }

        [Fact]
        public async Task UpsertTemplate_ShouldReturnForbidden_WhenTemplateBelongsToAnotherClub()
        {
            await LoginAsAdminAsync();

            var otherClubId = Guid.NewGuid();
            var ids = await _helpers.CreateFullTemplateHierarchyAsync(otherClubId);

            var command = new UpsertTemplateCommand(
                _testClubId,
                ids.templateId,
                "Hack Update",
                new List<StepDetailsDto>()
            );

            var response = await Client.PostAsJsonAsync(
                "/api/v1/admin/application-templates/upsert",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetTemplates_ReturnsPaginatedResults()
        {
            // Arrange
            await LoginAsAdminAsync();
            // Construct query string for pagination and clubId using correct route
            var url = $"/api/v1/clubs/{_testClubId}/application-templates?Page=1&Limit=10";

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("items", result); // Basic check for paginated structure
        }

        #region Delete Methods
        [Fact]
        public async Task DeleteTemplate_Requires_ClubId_In_Header()
        {
            // Arrange
            await LoginAsAdminAsync();

            // Create one to delete
            var (templateId, _, _, _) = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/application-templates/{templateId}");
            request.Headers.Add("X-Club-Id", _testClubId.ToString()); // Required by your controller

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        #endregion

        [Fact]
        public async Task Unauthorized_User_Cannot_Access_Templates()
        {
            // Arrange - Do NOT call Login
            var command = new UpsertTemplateCommand(
                _testClubId,
                null,
                "Secret Template",
                new List<StepDetailsDto>()
            );

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/admin/application-templates/upsert", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #region sync memPlans
        [Fact]
        public async Task SyncMembershipPlans_ShouldUpdateConnectedMembershipPlans()
        {
            await LoginAsAdminAsync();

            // Arrange
            var templateIds = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var membershipType = await _helpers.CreateMembershipTypeAsync(_testClubId, "Type A");
            var membershipPlan1 = await _helpers.CreateMembershipPlanAsync(_testClubId, membershipType.Id, "Plan 1", templateId: templateIds.templateId);
            var membershipPlan2 = await _helpers.CreateMembershipPlanAsync(_testClubId, membershipType.Id, "Plan 2", templateId: templateIds.templateId);

            var command = new SyncTemplateMembershipPlansCommand(
                templateIds.templateId,
                new List<Guid>
                {
                    membershipPlan1.Id,
                    membershipPlan2.Id
                });

            // Act
            var response = await PutSyncMembershipPlansAsync(templateIds.templateId, command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            ClearTracker();

            var template = await DbContext.ApplicationTemplateDefinitions
                .Include(t => t.ConnectedMembershipPlans)
                .FirstAsync(t => t.Id == templateIds.templateId);

            template.ConnectedMembershipPlans.Should().HaveCount(2);

            template.ConnectedMembershipPlans
                .Select(x => x.Id)
                .Should()
                .BeEquivalentTo(new[]
                {
                    membershipPlan1.Id,
                    membershipPlan2.Id
                });
        }

        [Fact]
        public async Task SyncMembershipPlans_ShouldRemoveMissingMembershipPlans()
        {
            await LoginAsAdminAsync();

            var templateIds = await _helpers.CreateFullTemplateHierarchyAsync(_testClubId);

            var membershipType = await _helpers.CreateMembershipTypeAsync(_testClubId, "Type A");
            var plan1 = await _helpers.CreateMembershipPlanAsync(_testClubId, membershipType.Id, "plan 1", templateId: templateIds.templateId);
            var plan2 = await _helpers.CreateMembershipPlanAsync(_testClubId, membershipType.Id, "plan 2", templateId: templateIds.templateId);
            var plan3 = await _helpers.CreateMembershipPlanAsync(_testClubId, membershipType.Id, "plan 3", templateId: templateIds.templateId);

            // initial sync with 3
            await PutSyncMembershipPlansAsync(
                templateIds.templateId,
                new SyncTemplateMembershipPlansCommand(
                    templateIds.templateId,
                    new() { plan1.Id, plan2.Id, plan3.Id }));

            ClearTracker();
            // second sync with only 1
            var response = await PutSyncMembershipPlansAsync(
                templateIds.templateId,
                new SyncTemplateMembershipPlansCommand(
                    templateIds.templateId,
                    new() { plan1.Id }));

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            ClearTracker();

            var template = await DbContext.ApplicationTemplateDefinitions
                .Include(t => t.ConnectedMembershipPlans)
                .FirstAsync(t => t.Id == templateIds.templateId);

            template.ConnectedMembershipPlans.Should().HaveCount(1);
            template.ConnectedMembershipPlans.First().Id.Should().Be(plan1.Id);
        }

        private async Task<HttpResponseMessage> PutSyncMembershipPlansAsync(Guid templateId, SyncTemplateMembershipPlansCommand command)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/application-templates/{templateId}/membership-plans")
            {
                Content = JsonContent.Create(command)
            };
            request.Headers.Add("X-Club-Id", _testClubId.ToString());
            return await Client.SendAsync(request);
        }
        #endregion

        #region Helpers
        private async Task<Guid> CreateTemplateAsync(string name)
        {
            var res = await Client.PostAsJsonAsync("/api/v1/admin/application-templates", new CreateTemplateCommand(_testClubId, name));
            return await res.Content.ReadFromJsonAsync<Guid>();
        }
        #endregion
    }
}
