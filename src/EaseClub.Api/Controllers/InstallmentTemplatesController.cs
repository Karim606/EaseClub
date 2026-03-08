using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate;
using EaseClub.Application.Features.InstallmentTemplates.Commands.UpdateTemplateList;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/{version:ApiVersion}/clubs/{clubId}/installment-templates")]
    public class InstallmentTemplatesController(ISender sender) : ApiController
    {

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("CreateTemplate")]
        [EndpointSummary("Creates a new installment template for the specified club.")]
        public async Task<IActionResult> CreateTemplate(Guid clubId, CreateInstallmentTemplateCommand request)
        {
            var command = new CreateInstallmentTemplateCommand(
                request.Name,
                request.NumOfInstallments,
                request.DurationInDays,
                request.Installments)
                { ClubId = clubId };

            var result = await sender.Send(command);
            return result.Match(
                id => CreatedAtAction(nameof(GetTemplate), new { version="1.0",clubId, id }, id),
                Problem);
        }

        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(InstallmentTemplateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetTemplate")]
        [EndpointSummary("Retrieves the details of a specific installment template.")]
        public async Task<IActionResult> GetTemplate(Guid id)
        {

            var query = new GetInstallmentTemplateQuery(id);
            var result = await sender.Send(query);

            return result.Match(
                  (template) => Ok(template),
                  Problem);
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(UnifiedPaginatedResponse<TemplatesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("ListInstallmentTemplates")]
        [EndpointDescription(@"
        ### Pagination Guide
        This endpoint supports two modes of pagination:
        1. **Offset:** Provide the `page` parameter to navigate by page number. Best for admin dashboards where total count is needed.
        2. **Cursor:** Provide the `cursor` parameter (received from the previous response) for performant, stable navigation. Best for mobile infinite scrolls.

        **Note:** If `cursor` is provided, the API will ignore the `page` parameter.")]
        [EndpointSummary("Lists all installment templates associated with a specific club.")]
        public async Task<IActionResult> ListByClub([FromQuery] Guid clubId, [FromQuery] PaginationRequest paginationDto)
        {
            var result = await sender.Send(new GetInstallmentTemplatesByClubQuery(clubId,paginationDto));
            return result.Match(
                (templates) => Ok(templates),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(Success), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("UpdateInstallmentList")]
        [EndpointSummary("Updates the list of installments for a specific template.")]
        public async Task<IActionResult> UpdateList(Guid id, [FromBody] List<InstallmentDto> installments)
        {
            var result = await sender.Send(new UpdateInstallmentListCommand(id, installments));
            return result.Match( _ => NoContent()
                , Problem);
        }
    }
}
