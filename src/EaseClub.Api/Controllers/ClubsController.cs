using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Features.Clubs.Commands.UpdateClubDetails;
using EaseClub.Application.Features.Clubs.Queries.GetClubById;
using EaseClub.Application.Features.Clubs.Queries.GetClubs;
using EaseClub.Application.Features.Clubs.Queries.GetAdminDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/clubs")]
    public class ClubsController(ISender sender) : ApiController
    {

        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(ClubResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("GetClubById")]
        [EndpointSummary("Gets a club by its unique identifier")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await sender.Send(new GetClubByIdQuery(id));

            return result.Match(
                (val) => Ok(val),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPut("/api/v{version:ApiVersion}/clubs/{id}/details")]
        [MapToApiVersion("1.0")]
        
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("UpdateClubDetails")]
        [EndpointSummary("Updates the details of a club, such as about, phone, email, work schedules, amenities, and logo. Requires ClubAdmin or SuperAdmin role.")]

        public async Task<IActionResult> UpdateDetails(Guid id, [FromBody] UpdateClubDetailsRequest request)
        {
            var command = new UpdateClubDetailsCommand(id, request);
            var result = await sender.Send(command);

           return result.Match(
                _ => NoContent(),
                Problem);

        }
        [HttpGet]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(ClubsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("GetClubs")]
        [EndpointSummary("list clubs for users , supported with cursor pagination")]
        public async Task<IActionResult> GetClubs([FromQuery]CursorPaginationParameters parameters)
        {
            var result = await sender.Send(new GetClubsQuery(parameters));

            return result.Match(
                (val) => Ok(val),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("{clubId}/admin-dashboard")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(ClubAdminDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetClubAdminDashboard")]
        [EndpointSummary("Gets the operational dashboard stats for a club administrator.")]
        public async Task<IActionResult> GetAdminDashboard(Guid clubId)
        {
            var result = await sender.Send(new GetAdminDashboardQuery(clubId));
            return result.Match(
                (val) => Ok(val),
                Problem);
        }

    }
}
