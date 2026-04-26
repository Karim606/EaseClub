using EaseClub.Application.Features.Users.Queries.GetClubAdminContext;
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
