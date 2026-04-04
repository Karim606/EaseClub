using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Features.Clubs.Queries.GetClubById;
using EaseClub.Application.Features.Clubs.Queries.GetClubs;
using MediatR;
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

        [HttpGet]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(ClubResponse), StatusCodes.Status200OK)]
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

    }
}
