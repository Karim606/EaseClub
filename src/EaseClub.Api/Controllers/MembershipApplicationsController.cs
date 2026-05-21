using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipApplications;
using EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep;
using EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication;
using EaseClub.Application.Features.MembershipApplications.Commands.SubmitApplication;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplication;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationForAdmin;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplications;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationsForManagement;
using EaseClub.Application.Features.MembershipApplications.Queries.GetPricingForApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        [ProducesResponseType(typeof(ApplicationUserResponse), StatusCodes.Status200OK)]
        [EndpointName("GetApplication")]
        [EndpointSummary("Retrieves the details of a specific application.")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await sender.Send(new GetApplicationQuery(id));
            return result.Match(
                val => Ok(val),
                Problem);
        }

        [HttpGet("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApplicationAdminResponse), StatusCodes.Status200OK)]
        [EndpointName("GetApplicationForAdmin")]
        [EndpointSummary("Retrieves the details of a specific application.")]
        public async Task<IActionResult> GetApplicationForAdmin(Guid id)
        {
            var result = await sender.Send(new GetApplicationAdminQuery(id));
            return result.Match(
                val => Ok(val),
                Problem);
        }

        [HttpGet("{id:guid}/pricing")]
        [ProducesResponseType(typeof(Pricing), StatusCodes.Status200OK)]
        [EndpointName("GetApplicationPricing")]
        [EndpointSummary("Retrieves the details of a specific application.")]
        public async Task<IActionResult> GetApplicationPricing(Guid id)
        {
            var result = await sender.Send(new GetApplicationPricingQuery(id));
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

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId}/membership-applications")]
        [ProducesResponseType(typeof(OffsetPaginatedResult<MembershipAppAdminDto>), StatusCodes.Status200OK)]
        [EndpointName("GetApplicationsForManagement")]
        [EndpointSummary("Lists applications for admins based on filters criteria.")]
        public async Task<IActionResult> GetApplicationsForManagement(Guid clubId,[FromQuery] GetApplicationsForManagementQuery query)
        {
            query = query with { ClubId = clubId };
            var result = await sender.Send(query);
            return result.Match(
                (apps) => Ok(apps),
                Problem);
        }


        [HttpPost("{id}/steps/{order}/complete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [EndpointName("CompleteStep")]
        [EndpointSummary("Submits answers for a specific application step.")]
        #region swagger-description
        [EndpointDescription(@"
Completes a specific application step by submitting field answers.

This endpoint:
- Validates the step order against application progression rules
- Validates each field against the frozen template snapshot
- Removes previous answers for the same step before applying new ones
- Stores validated answers and updates application progress
- Recalculates application pricing after step completion

Rules:
- Application must be in Draft status
- Steps must be completed in sequence
- Each answer is validated using field-level validation rules from the template snapshot

Answer behavior:
- FieldId must exist in the step snapshot
- InstanceIndex is used for repeatable fields (e.g. family members)
- Invalid fields are ignored or rejected depending on validation rules

Side effects:
- Updates application answers
- Advances application step pointer
- Updates pricing dynamically
")]

        #endregion
        public async Task<IActionResult> CompleteStep(Guid id, int order, [FromBody] List<AnswerRequestDto> answers) =>
        (await sender.Send(new CompleteStepCommand(id, order, answers))).Match(_ => NoContent(), Problem);

        [HttpPost("{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointName("SubmitApplication")]
        [EndpointSummary("Finalizes and submits the application for review.")]
        #region swagger-description
        [EndpointDescription(@"
Finalizes and submits a membership application for processing.

This is the terminal state transition of the application workflow.

What this endpoint does:

1. Loads the application with all completed steps and answers
2. Validates the application through domain rules:
   - Ensures all required steps are completed
   - Validates section constraints
   - Locks pricing based on final answers
3. Marks the application as SUBMITTED (no further edits allowed)
4. Locks all dynamic data into a final immutable state
5. Processes file attachments and marks uploaded files as permanent

File Handling:
- Any answer of type 'File' is treated as a temporary upload reference
- On submission, files are promoted to permanent storage
- Temporary uploads not referenced by answers may be eligible for cleanup

Business Rules:
- Application must be in DRAFT state
- No further modifications are allowed after submission
- Price is frozen at submission time
- Answers are treated as immutable snapshot of user input

Side Effects:
- Locks application lifecycle
- Finalizes pricing calculation
- Converts uploaded files to permanent storage
- Triggers downstream workflows (review, approval, billing, etc.)

Important:
This operation is irreversible from a business perspective.
")]

        #endregion
        public async Task<IActionResult> Submit(Guid id) =>
            (await sender.Send(new SubmitApplicationCommand(id))).Match(_ => Ok(), Problem);

        [Authorize(Roles ="ClubAdmin,SuperAdmin")]
        [HttpPost("{id}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointName("ReviewApplication")]
        [EndpointSummary("Approves or rejects a submitted application.")]
        [EndpointDescription("Approves or rejects a submitted application, " +
            "values you can send for decision enum are  Approved or Rejected")]
        public async Task<IActionResult> Review([FromRoute]Guid id, [FromBody] ReviewApplicationCommand cmd)
        {

            var result = await sender.Send(cmd with { ApplicationId = id });

            return result.Match((_)=>Ok(), Problem);
        }


    }
}
