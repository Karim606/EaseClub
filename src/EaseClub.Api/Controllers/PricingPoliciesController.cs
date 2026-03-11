using EaseClub.Application.Features.PricingPolicies.Commands.CreatePolicy;
using EaseClub.Application.Features.PricingPolicies.Commands.DeletePolicy;
using EaseClub.Application.Features.PricingPolicies.Commands.UpdatePolicy;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyByClubId;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/clubs/{clubId:guid}/pricing-policies")]
    public class PricingPoliciesController(ISender sender) : ApiController
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(Guid clubId, CreatePricingPolicyCommand command)
        {
            var result = await sender.Send(command with { ClubId = clubId });
            return result.Match(
                id => CreatedAtAction(nameof(GetById), new { version = "1.0", id }, id),
                Problem);
        }

        [HttpGet("/api/v{version:ApiVersion}/pricing-policies/{id:guid}")]
        [ProducesResponseType(typeof(PricingPolicyResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await sender.Send(new GetPricingPolicyByIdQuery(id));
            return result.Match(
                (policy) => Ok(policy),
                Problem);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PricingPolicyResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByClub(Guid clubId)
        {
            var result = await sender.Send(new GetPricingPoliciesByClubQuery(clubId));
            return result.Match(
                (policies) => Ok(policies),
                Problem);
        }

        [HttpPut("/api/v{version:ApiVersion}/pricing-policies/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(Guid id, UpdatePricingPolicyCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");

            var result = await sender.Send(command);
            return result.Match(
                _ => NoContent(),
                Problem);
        }

        [HttpDelete("/api/v{version:ApiVersion}/pricing-policies/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await sender.Send(new DeletePricingPolicyCommand(id));
            return result.Match(
                _ => NoContent(),
                Problem);
        }

    }
}
