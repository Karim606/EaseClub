using EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.RemoveAnswer;
using EaseClub.Application.Features.MembershipApplications.Commands.UpdateAnswer;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplication;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/membership-applications")]
    public class MembershipApplicationsController(ISender sender) : ApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApplicationCommand command,CancellationToken ct)
        {
            var result = await sender.Send(command,ct);
            return result.Match(id => CreatedAtAction(nameof(Get), new { id }, id), Problem);
        }

        // Get the application structure and answers
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await sender.Send(new GetApplicationQuery(id));
            return result.Match(
                val => Ok(val),
                Problem);
        }

        // Update an answer (The one we built in the previous step)
        [HttpPatch("{id}/answers")]
        public async Task<IActionResult> UpdateAnswer(Guid id, [FromBody] SetAnswerCommand command,CancellationToken ct)
        {
            var result = await sender.Send(command with { ApplicationId = id },ct);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpDelete("{applicationId}/answers/{fieldDefinitionId}")]
        public async Task<IActionResult> RemoveAnswer(
        Guid applicationId,
        Guid fieldDefinitionId,
        CancellationToken ct,
        [FromQuery] int index = 0
        )
        {
            var result = await sender.Send(new RemoveAnswerCommand(applicationId, fieldDefinitionId, index),ct);
            return result.Match(_ => NoContent(), Problem);
        }
    }
}
