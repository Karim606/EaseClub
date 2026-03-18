using EaseClub.Application.Features.Branches.Commands.CreateBranch;
using EaseClub.Application.Features.Branches.Commands.DeleteBranch;
using EaseClub.Application.Features.Branches.Commands.EditBranch;
using EaseClub.Application.Features.Branches.Queries.GetBranchById;
using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Application.Features.Branches.Queries.GetBranchesForAdmins;
using EaseClub.Application.Features.Clubs.Queries.GetClubById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/branches")]
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

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("management")]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(List<BranchAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("GetBranchesByClubForManagement")]
        [EndpointSummary("Get Branches by it club's unique identifier for admins.")]
        public async Task<IActionResult> GetBranchesForManagement(Guid clubId,bool? isActive)
        {
            var result = await sender.Send(new GetBranchesForAdminQuery(clubId,isActive));

            return result.Match(
                (branches) => Ok(branches),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPost]
        [MapToApiVersion("1.0")]

        [ProducesResponseType(typeof(Guid),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]

        [EndpointName("CreateBranch")]
        [EndpointSummary("Creates a new branch for a specific club.")]

        public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
        {
            var result = await sender.Send(new CreateBranchCommand(request.clubId,request.Name));

           return result.Match(
                (id) =>{return CreatedAtAction(nameof(GetByClub), new { version = "1.0", request.clubId }, id);},
                Problem);
        }

        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
        [EndpointName("GetBranch")]
        [EndpointSummary("Get Branch by it unique identifier.")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await sender.Send(new GetBranchByIdQuery(id));

            return result.Match(
                (branch) => Ok(branch),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [EndpointName("EditBranch")]
        [EndpointSummary("Edit the details of an existing branch.")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditBranchRequest request)
        {
            var result = await sender.Send(new EditBranchCommand(id, request.Name));

            return result.Match(
                _ => NoContent(),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [EndpointName("DeleteBranch")]
        [EndpointSummary("Remove a branch from the club.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await sender.Send(new DeleteBranchCommand(id));

            return result.Match(
                (_) => NoContent(),
                Problem);
        }
    }
}
