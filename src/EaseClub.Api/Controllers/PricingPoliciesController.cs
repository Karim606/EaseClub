using EaseClub.Application.Features.PricingPolicies.Commands.AssignPolicy;
using EaseClub.Application.Features.PricingPolicies.Commands.CreatePolicy;
using EaseClub.Application.Features.PricingPolicies.Commands.DeletePolicy;
using EaseClub.Application.Features.PricingPolicies.Commands.UnAssignPolicy;
using EaseClub.Application.Features.PricingPolicies.Commands.UpdatePolicy;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPoliciesAssignmentsByTarget;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyByClubId;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById;
using EaseClub.Domain.PricingPolices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [Route("api/v{version:ApiVersion}/clubs/{clubId:guid}/pricing-policies")]
    public class PricingPoliciesController(ISender sender) : ApiController
    {
        [HttpPost]
        [EndpointName("CreatePricingPolicy")]
        [EndpointSummary("Create a new pricing policy.")]
        [EndpointDescription("Creates a new pricing policy for a specific club with rules, conditions, and pricing behavior.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(Guid clubId, CreatePricingPolicyCommand command)
        {
            var result = await sender.Send(command with { ClubId = clubId });
            return result.Match(
                id => CreatedAtAction(nameof(GetById), new { version = "1.0", id }, id),
                Problem);
        }

        [HttpGet("/api/v{version:ApiVersion}/pricing-policies/{id:guid}")]
        [EndpointName("GetPricingPolicyById")]
        [EndpointSummary("Retrieve a pricing policy by ID.")]
        [EndpointDescription("Fetches a single pricing policy including its rules and conditions.")]
        [ProducesResponseType(typeof(PricingPolicyResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await sender.Send(new GetPricingPolicyByIdQuery(id));
            return result.Match(
                (policy) => Ok(policy),
                Problem);
        }

        [HttpGet]
        [EndpointName("GetPricingPoliciesByClub")]
        [EndpointSummary("Get all pricing policies for a club.")]
        [EndpointDescription("Returns pricing policies for a club. Pass ?compatibleWith=Event or ?compatibleWith=ApplicationTemplate&compatibleWithTargetId={id} to filter by target compatibility.")]
        [ProducesResponseType(typeof(List<PricingPolicyResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByClub(
            Guid clubId,
            [FromQuery] PricingPolicyTargetType? compatibleWith = null,
            [FromQuery] Guid? compatibleWithTargetId = null)
        {
            var result = await sender.Send(new GetPricingPoliciesByClubQuery(clubId, compatibleWith, compatibleWithTargetId));
            return result.Match(
                (policies) => Ok(policies),
                Problem);
        }

        [HttpPut("/api/v{version:ApiVersion}/pricing-policies/{id:guid}")]
        [EndpointName("UpdatePricingPolicy")]
        [EndpointSummary("Update an existing pricing policy.")]
        [EndpointDescription("Updates pricing policy details such as conditions, priority, and calculation rules.")]
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
        [EndpointName("DeletePricingPolicy")]
        [EndpointSummary("Delete a pricing policy.")]
        [EndpointDescription("Deletes a pricing policy permanently from the system.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await sender.Send(new DeletePricingPolicyCommand(id));
            return result.Match(
                _ => NoContent(),
                Problem);
        }

        [HttpPost("/api/v{version:ApiVersion}/pricing-policies/assign")]
        [EndpointName("AssignPricingPolicy")]
        [EndpointSummary("Assign pricing policies to a target.")]
        [EndpointDescription("Assigns a pricing policy to a specific target (e.g., Application Template) with a defined priority.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignPricingPolicies([FromBody] AssignPoliciesCommand command)
        {
            var result = await sender.Send(command);
            return result.Match(
                _ => NoContent(),
                Problem);
        }

        [HttpPost("/api/v{version:ApiVersion}/pricing-policies/unassign")]
        [EndpointName("UnAssignPricingPolicy")]
        [EndpointSummary("Remove a pricing policy from a target.")]
        [EndpointDescription("Removes an assigned pricing policy from a specific target.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnAssignPricingPolicy([FromBody] UnAssignPolicyCommand command)
        {
            var result = await sender.Send(command);
            return result.Match(
                _ => NoContent(),
                Problem);
        }

        [HttpGet("/api/v{version:ApiVersion}/pricing-policies/{targetType}/targets/{targetId:guid}/assignments")]
        public async Task<IActionResult> GetPoliciesAssignmentsByTargetId(Guid targetId, PricingPolicyTargetType targetType)
        {
            var result = await sender.Send(new GetPoliciesAssignmentsByTargetQuery(targetId, targetType));
            return result.Match(
                (policies) => Ok(policies),
                Problem);

        }
    }
}
