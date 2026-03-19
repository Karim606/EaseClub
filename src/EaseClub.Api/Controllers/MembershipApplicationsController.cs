using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipApplications;
using EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep;
using EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.RemoveAnswer;
using EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.SubmitApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.UpdateAnswer;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplication;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplications;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationsForManagement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/membership-applications")]
    public class MembershipApplicationsController(ISender sender) : ApiController
    {
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [EndpointName("CreateApplication")]
        [EndpointSummary("Starts a new membership application.")]
        public async Task<IActionResult> Create([FromBody] CreateApplicationCommand command,CancellationToken ct)
        {
            var result = await sender.Send(command,ct);
            return result.Match(id => CreatedAtAction(nameof(Get), new { id }, id), Problem);
        }

        // Get the application structure and answers
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
        [EndpointName("GetApplication")]
        [EndpointSummary("Retrieves the details of a specific application.")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await sender.Send(new GetApplicationQuery(id));
            return result.Match(
                val => Ok(val),
                Problem);
        }

        [HttpGet]
        [ProducesResponseType(typeof(UnifiedPaginatedResponse<MembershipAppDto>), StatusCodes.Status200OK)]
        [EndpointName("GetApplications")]
        [EndpointSummary("Lists applications based on filter criteria.")]
        public async Task<IActionResult> GetApplications([FromQuery] GetApplicationsQuery query)
        {
            var result = await sender.Send(query);
            return result.Match(
                (apps) => Ok(apps),
                Problem);
        }

        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId}/membership-applications")]
        [ProducesResponseType(typeof(OffsetPaginatedResult<MembershipAppDto>), StatusCodes.Status200OK)]
        [EndpointName("GetApplicationsForManagement")]
        [EndpointSummary("Lists applications for admins based on filters criteria.")]
        public async Task<IActionResult> GetApplicationsForManagement([FromQuery] GetApplicationsForManagementQuery query)
        {
            var result = await sender.Send(query);
            return result.Match(
                (apps) => Ok(apps),
                Problem);
        }


        [HttpPost("{id}/steps/{order}/complete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [EndpointName("CompleteStep")]
        [EndpointSummary("Submits answers for a specific application step.")]
        public async Task<IActionResult> CompleteStep(Guid id, int order, [FromBody] List<AnswerDto> answers) =>
        (await sender.Send(new CompleteStepCommand(id, order, answers))).Match(_ => NoContent(), Problem);

        [HttpPost("{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointName("SubmitApplication")]
        [EndpointSummary("Finalizes and submits the application for review.")]
        public async Task<IActionResult> Submit(Guid id) =>
            (await sender.Send(new SubmitApplicationCommand(id))).Match(_ => Ok(), Problem);

        [Authorize(Roles ="ClubAdmin,SuperAdmin")]
        [HttpPost("{id}/reviews")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [EndpointName("ReviewApplication")]
        [EndpointSummary("Approves or rejects a submitted application.")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewApplicationDto dto)
        {
            var command = new ReviewApplicationCommand(
                id,
                dto.Decision,
                dto.Reason
            );

            var result = await sender.Send(command);

            return result.Match(_ => NoContent(), Problem);
        }


        // Update an answer (The one we built in the previous step)
        //[HttpPatch("{id}/answers")]
        //public async Task<IActionResult> UpdateAnswer(Guid id, [FromBody] SetAnswerCommand command,CancellationToken ct)
        //{
        //    var result = await sender.Send(command with { ApplicationId = id },ct);
        //    return result.Match(_ => NoContent(), Problem);
        //}

        //[HttpDelete("{applicationId}/answers/{fieldDefinitionId}")]
        //public async Task<IActionResult> RemoveAnswer(
        //Guid applicationId,
        //Guid fieldDefinitionId,
        //CancellationToken ct,
        //[FromQuery] int index = 0
        //)
        //{
        //    var result = await sender.Send(new RemoveAnswerCommand(applicationId, fieldDefinitionId, index),ct);
        //    return result.Match(_ => NoContent(), Problem);
        //}
    }
}
