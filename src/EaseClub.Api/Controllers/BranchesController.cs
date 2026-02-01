using EaseClub.Application.Features.Branches.Commands.CreateBranch;
using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Application.Features.Clubs.Queries.GetClubById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/clubs/{clubId}/branches")]
    public class BranchesController(ISender sender) : ApiController
    {

        [HttpGet]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(List<BranchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("GetBranchesByClub")]
        [EndpointSummary("Get Branches by it club's unique identifier.")]
        public async Task<IActionResult> GetByClub(Guid clubId)
        {
            var result = await sender.Send(new GetBranchesByClubQuery(clubId));

            return result.Match(
                (branches) => Ok(branches),
                Problem);
        }

    }
}
