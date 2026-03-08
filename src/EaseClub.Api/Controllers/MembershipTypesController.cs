using EaseClub.Application.Features.MembershipTypes;
using EaseClub.Application.Features.MembershipTypes.Commands.CreateMembershipType;
using EaseClub.Application.Features.MembershipTypes.Commands.ToggleMembershipTypeActivation;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesByClub;
using EaseClub.Application.Features.MembershipTypes.Queries.GetTypeById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/clubs/{clubId}/membership-types")]
    public class MembershipTypesController(ISender sender) : ApiController
    {

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPost]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(Guid),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [EndpointName("CreateMembershipType")]
        [EndpointSummary("Creates a new membership type for the specified club.")]

        public async Task<IActionResult> Create(Guid clubId, CreateMembershipTypeCommand cmd)
        {
            var result = await sender.Send(cmd with { ClubId = clubId });

           return result.Match(
                (id) => Ok(id),
                Problem);
        }

        [HttpGet]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(List<MembershipTypeDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [EndpointName("GetMembershipTypesByClub")]
        [EndpointSummary("Retrieves all membership types associated with the specified club.")]
        public async Task<IActionResult> Get(Guid clubId)
        {
            var result = await sender.Send(new GetMembershipTypesByClubQuery(clubId));

            return result.Match(
                (membershipTypes) => Ok(membershipTypes),
                Problem);
        }

        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(MembershipTypeDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMembershipType")]
        [EndpointSummary("Retrieves details of a specific membership type.")]
        public async Task<IActionResult> GetById(Guid clubId, Guid id)
        {
            var result = await sender.Send(new GetMembershipTypeByIdQuery(clubId, id));

            return result.Match(
                type => Ok(type),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPatch("{id:guid}/toggle-status")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("ToggleMembershipTypeStatus")]
        [EndpointSummary("Toggles the activation status (Active/Inactive) of a membership type.")]
        public async Task<IActionResult> ToggleStatus(Guid clubId, Guid id)
        {
            var result = await sender.Send(new ToggleTypeActivationCommand(clubId, id));

            return result.Match(
                isActive => Ok(isActive),
                Problem);
        }
    }
}
