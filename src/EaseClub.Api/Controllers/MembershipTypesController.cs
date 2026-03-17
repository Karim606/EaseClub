using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipTypes;
using EaseClub.Application.Features.MembershipTypes.Commands.CreateMembershipType;
using EaseClub.Application.Features.MembershipTypes.Commands.ToggleMembershipTypeActivation;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipsTypesForMember;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesForAdmin;
using EaseClub.Application.Features.MembershipTypes.Queries.GetTypeById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/membership-types")]
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

        public async Task<IActionResult> Create( [FromBody]CreateMembershipTypeCommand cmd)
        {
            var result = await sender.Send(cmd);

           return result.Match(
                (id) => Ok(id),
                Problem);
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(UnifiedPaginatedResponse<MembershipTypeDto>), StatusCodes.Status200OK)]
        [EndpointName("GetMembershipTypesForMember")]
        [EndpointSummary("Retrieves all active membership types accessible.")]
        [EndpointDescription("Returns a list of membership types to members," +
            "it supports (offset+cursor) pagination choose one of them , it also supports filtering by branch and access to all branches.")]
        public async Task<IActionResult> GetMembershipTypesForMember([FromQuery] Guid? clubId,[FromQuery] Guid? branchId,[FromQuery] bool? accessToAllBranches, [FromQuery] PaginationRequest pagination, CancellationToken ct)
        {

            var query = new GetMembershipTypesForMemberQuery(clubId, branchId, accessToAllBranches, pagination);
            var result = await sender.Send(query, ct);
            return result.Match(list => Ok(list), 
                Problem);
        }

        [Route("/api/v{version:ApiVersion}/clubs/{clubId}/membership-types")]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [ProducesResponseType(typeof(List<MembershipTypeAdminDto>), StatusCodes.Status200OK)]
        [EndpointName("GetMembershipTypesForAdmin")]
        [EndpointSummary("Retrieves all membership types for specific club , authorized for ClubAdmin and SuperAdmin users\n clubadmin can only retrieve membership types of their assigned club.")]
        [EndpointDescription(@"
### Endpoint Description
This endpoint allows users to retrieve membership types for their assigned club.  
Only **active membership types** are returned for security and simplicity.

### Access Rules
- Users can only access membership types of their assigned club.

### Query Parameters
- `branchId` (optional): Filter membership types by branch.
- `accessToAllBranches` (optional): Include types available to all branches.
- Pagination parameters: `pageNumber`, `pageSize` to control paging.

### Pagination Guide
This endpoint supports two modes of pagination:
1. **Offset:** Provide the `page` parameter to navigate by page number. Best for admin dashboards where total count is needed.
2. **Limit:** Provide the `limit` parameter to limit the number of items per page.")]
        public async Task<IActionResult> GetMembershipTypesForAdmin(Guid clubId, [FromQuery] Guid? branchId, [FromQuery] bool? accessToAllBranches, [FromQuery] bool? isActive, [FromQuery] PaginationRequest pagination, CancellationToken ct)
        {
            var query = new GetMembershipTypesForAdminQuery(clubId, branchId, accessToAllBranches, isActive, pagination);
            var result = await sender.Send(query, ct);
            return result.Match(list => Ok(list),
                Problem);
        }

        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(MembershipTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMembershipType")]
        [EndpointSummary("Retrieves details of a specific membership type.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await sender.Send(new GetMembershipTypeByIdQuery( id));

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
