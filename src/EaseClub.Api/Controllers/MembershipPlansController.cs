using EaseClub.Application.Features.MembershipPlans.Command.AddTemplateToPlan;
using EaseClub.Application.Features.MembershipPlans.Command.CreatePlan;
using EaseClub.Application.Features.MembershipPlans.Command.RemoveInstallmentTemplateFromPlan;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
using EaseClub.Domain.Clubs;
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
        public async Task<IActionResult> GetByClub(Guid clubId, CancellationToken ct)
        {
            // The ClubId is pulled from the URL route
            var query = new GetMembershipPlansByClubQuery(clubId);

            var result = await sender.Send(query, ct);

            return result.Match(
                (plans) => Ok(plans),
                Problem);
        }

        [Authorize(Roles ="ClubAdmin")]
        [HttpPost]
        [MapToApiVersion("1.0")]
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

        [Authorize(Roles = "ClubAdmin")]
        [HttpPost("{planId}/templates/{templateId}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> AddTemplateToPlan(Guid clubId,Guid planId, Guid templateId)
        {
            var result = await sender.Send(new AddInstallmentTemplateToPlanCommand(clubId,planId, templateId));
            return  result.Match(
                (val) => NoContent(),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin")]
        [HttpDelete("{planId}/templates/{templateId}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> RemoveTemplateFromPlan(Guid clubId, Guid planId, Guid templateId)
        {
            var result = await sender.Send(new RemoveInstallmentTemplateFromPlanCommand(clubId,planId, templateId));
            return result.Match(
                (val) => NoContent(),
                Problem);
        }
    }
}
