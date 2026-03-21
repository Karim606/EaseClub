using EaseClub.Application.Features.Files.Commands;
using EaseClub.Application.Features.Files.Commands.UploadApplicationFile;
using EaseClub.Application.Features.Files.Commands.UploadClubFile;
using EaseClub.Application.Features.Files.Queries.GetFileQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/files")]
    [Authorize]
    public class FilesController(ISender sender) : ApiController
    {

        [HttpPost("application")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(SecureFileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [EndpointName("UploadApplicationFile")]
        [EndpointSummary("Upload a file for a membership application.")]
        public async Task<IActionResult> UploadApplicationFile([FromForm] UploadApplicationFileCommand command)
        {
            var result = await sender.Send(command);

            return result.Match(
                file => Ok(file),
                Problem
            );
        }

        [HttpPost("club")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(SecureFileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [EndpointName("UploadClubFile")]
        [EndpointSummary("Upload a file for a club.")]
        public async Task<IActionResult> UploadClubFile([FromForm] UploadClubFileCommand command)
        {
            var result = await sender.Send(command);

            return result.Match(
                file => Ok(file),
                Problem
            );
        }

        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(SecureFileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetFileById")]
        [EndpointSummary("Retrieve file metadata by its unique identifier.")]
        public async Task<IActionResult> GetFileById(Guid id)
        {
            var result = await sender.Send(new GetFileQuery(id));

            return result.Match(
                file => Ok(file),
                Problem
                );
        }



    }

}
