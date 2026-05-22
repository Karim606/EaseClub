using EaseClub.Application.Features.Enrollments;
using EaseClub.Application.Features.Enrollments.Commands.Cancel;
using EaseClub.Application.Features.Enrollments.Commands.StartDirectPay;
using EaseClub.Application.Features.Enrollments.Commands.StartRenewal;
using EaseClub.Application.Features.Enrollments.Queries.GetActive;
using EaseClub.Application.Features.Enrollments.Queries.GetSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/enrollments")]
    public class EnrollmentsController(ISender sender) : ApiController
    {
        [Authorize(Roles = "Member")]
        [HttpPost("direct")]
        [ProducesResponseType(typeof(EnrollmentPaymentResponse), StatusCodes.Status200OK)]
        [EndpointName("StartDirectEnrollment")]
        [EndpointSummary("Initiates a new membership enrollment flow.")]
        public async Task<IActionResult> StartDirectPay([FromBody] StartDirectPayEnrollmentCommand command, CancellationToken ct)
        {
            var result = await sender.Send(command, ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "Member")]
        [HttpPost("renewal")]
        [ProducesResponseType(typeof(EnrollmentPaymentResponse), StatusCodes.Status200OK)]
        [EndpointName("StartRenewalEnrollment")]
        [EndpointSummary("Initiates a membership renewal flow.")]
        public async Task<IActionResult> StartRenewal([FromBody] StartRenewalEnrollmentCommand command, CancellationToken ct)
        {
            var result = await sender.Send(command, ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "Member")]
        [HttpGet("active/{memberId:guid}")]
        [ProducesResponseType(typeof(List<ActiveEnrollmentDto>), StatusCodes.Status200OK)]
        [EndpointName("GetActiveEnrollment")]
        [EndpointSummary("Retrieves the user's current active enrollment intents.")]
        public async Task<IActionResult> GetActiveEnrollment(Guid memberId, CancellationToken ct)
        {
            var result = await sender.Send(new GetActiveEnrollmentQuery(memberId), ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "Member")]
        [HttpGet("{id:guid}/summary")]
        [ProducesResponseType(typeof(EnrollmentSummaryDto), StatusCodes.Status200OK)]
        [EndpointName("GetEnrollmentSummary")]
        [EndpointSummary("Retrieves the pricing and installment summary for an enrollment.")]
        public async Task<IActionResult> GetEnrollmentSummary(Guid id, CancellationToken ct)
        {
            var result = await sender.Send(new GetEnrollmentSummaryQuery(id), ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "Member")]
        [HttpDelete("{id:guid}")]
        [EndpointName("CancelEnrollment")]
        [EndpointSummary("Cancels an active enrollment intent.")]
        public async Task<IActionResult> CancelEnrollment(Guid id, CancellationToken ct)
        {
            var result = await sender.Send(new CancelEnrollmentCommand(id), ct);
            return result.Match(_ => NoContent(), Problem);
        }
    }
}
