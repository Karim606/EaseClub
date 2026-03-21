using EaseClub.Application.Features.Users.Queries.GetClubAdminContext;
using EaseClub.Application.Features.Users.Queries.GetUserMemberClubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/users/me")]
    [Authorize]
    public class UsersController(ISender sender) : ApiController
    {
        [HttpGet("memberships")]
        [EndpointSummary("Retrieves all clubs where the current user is a member.")]
        [EndpointDescription("Returns a list of club IDs and names that the authenticated user is currently a member of, used for filtering applications and viewing status.")]
        [ProducesResponseType(typeof(List<UserClubResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyMemberships()
        {
            // Extension method to get ID from JWT Claims
            var result = await sender.Send(new GetUserMemberClubsQuery());
            return result.Match(
                (val) =>Ok(val)
                , Problem);
        }

        [HttpGet("admin-context")]
        [EndpointSummary("Retrieves the administrative context for the current user.")]
        [EndpointDescription("Returns the ClubAdminId and the associated ManagedClubId, providing necessary context for building administrative requests")]
        [ProducesResponseType(typeof(ClubAdminContextResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetMyAdminContext()
        {
            var result = await sender.Send(new GetClubAdminContextQuery());
            return result.Match(
                (val) => Ok(val), 
                Problem);
        }
    }
}
