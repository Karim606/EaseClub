using EaseClub.Application.Features.Clubs.Queries.GetClubById;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/clubs")]
    public class ClubsController(ISender sender) : ApiController
    {

        [HttpGet("{id}")]
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
    }
}
