using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Features.MembershipPlans.Command.AddTemplateToPlan;
using EaseClub.Application.Features.MembershipPlans.Command.CreatePlan;
using EaseClub.Application.Features.MembershipPlans.Command.DeleteMembershipPlan;
using EaseClub.Application.Features.MembershipPlans.Command.RemoveInstallmentTemplateFromPlan;
using EaseClub.Application.Features.MembershipPlans.Command.UpdatePlan;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlanDetails;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{

    [Route("api/v{version:ApiVersion}/clubs/{clubId}/membership-plans")]
    public class MembershipPlansController(ISender sender) : ApiController
    {

        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<MembershipPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointDescription(@"
        ### Pagination Guide
        This endpoint supports two modes of pagination:
        1. **Offset:** Provide the `page` parameter to navigate by page number. Best for admin dashboards where total count is needed.

        you can send also `membershipTypeId` to filter by membership type id.` ")]
        [EndpointSummary("Lists all membership plans associated with a specific club.")]
        public async Task<IActionResult> GetByClub(Guid clubId, [FromQuery] Guid? membershipTypeId,
            [FromQuery]OffsetPaginationParameters paginationParameters,
            CancellationToken ct)
        {
            // The ClubId is pulled from the URL route
            var query = new GetMembershipPlansByClubQuery(clubId,membershipTypeId,paginationParameters);

            var result = await sender.Send(query, ct);

            return result.Match(
                (plans) => Ok(plans),
                Problem);
        }

        [Authorize(Roles ="ClubAdmin,SuperAdmin")]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("CreateMembershipPlan")]
        [EndpointSummary("Creates a new membership plan for the specified club.")]

        public async Task<IActionResult> CreatePlan(Guid clubId, CreatePlanRequest request)
        {
            var command = new CreateMembershipPlanCommand(
                clubId,
                request.MembershipTypeId,
                request.Name,
                request.Price,
                request.DurationInDays);

            var result = await sender.Send(command);
            return result.Match(
                id => CreatedAtAction(nameof(GetByClub), new { version = "1.0", clubId },id),
                Problem);
        }

        [HttpGet("{planId:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(MembershipPlanDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMembershipPlan")]
        [EndpointSummary("Retrieves the full details of a membership plan, including linked installment templates.")]
        public async Task<IActionResult> GetById(Guid planId)
        {
            var result = await sender.Send(new GetMembershipPlanDetailsQuery(planId));

            return result.Match( (plan) =>Ok(plan)
                , Problem);
        }


        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPut("{planId:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("UpdateMembershipPlanWithTemplates")]
        [EndpointSummary("Updates plan details and synchronizes its associated installment templates.")]
        public async Task<IActionResult> Update(Guid clubId, Guid planId, [FromBody] UpdateMembershipPlanCommand command)
        {

            var result = await sender.Send(command);
            return result.Match(_ => NoContent()
                , Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {

            var result = await sender.Send(new DeleteMembershipPlanCommand(id));

            return result.Match(_ => NoContent(), Problem);
        }

        //[Authorize(Roles = "ClubAdmin")]
        //[HttpPost("{planId}/templates/{templateId}")]
        //[MapToApiVersion("1.0")]
        //public async Task<IActionResult> AddTemplateToPlan(Guid clubId,Guid planId, Guid templateId)
        //{
        //    var result = await sender.Send(new AddInstallmentTemplateToPlanCommand(clubId,planId, templateId));
        //    return  result.Match(
        //        (val) => NoContent(),
        //        Problem);
        //}

        //[Authorize(Roles = "ClubAdmin")]
        //[HttpDelete("{planId}/templates/{templateId}")]
        //[MapToApiVersion("1.0")]
        //public async Task<IActionResult> RemoveTemplateFromPlan(Guid clubId, Guid planId, Guid templateId)
        //{
        //    var result = await sender.Send(new RemoveInstallmentTemplateFromPlanCommand(clubId,planId, templateId));
        //    return result.Match(
        //        (val) => NoContent(),
        //        Problem);
        //}
    }
}
