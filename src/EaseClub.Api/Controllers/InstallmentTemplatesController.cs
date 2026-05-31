using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate;
using EaseClub.Application.Features.InstallmentTemplates.Commands.UpdateTemplateList;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplatesForManagement;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/installment-templates")]
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
        public async Task<IActionResult> CreateTemplate( CreateInstallmentTemplateCommand request)
        {
            var command = new CreateInstallmentTemplateCommand(
                request.ClubId,
                request.Name,
                request.NumOfInstallments,
                request.DurationInDays,
                request.Installments);

            var result = await sender.Send(command);
            return result.Match(
                id => CreatedAtAction(nameof(GetTemplate), new { version="1.0",request.ClubId, id }, id),
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
        [ProducesResponseType(typeof(List<TemplatesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("ListInstallmentTemplates")]
        [EndpointSummary("Retrieves the list installment template, supported filters is clubId and planId")]
        public async Task<IActionResult> ListByClub([FromQuery] Guid? clubId, [FromQuery] Guid? planId)
        {
            var result = await sender.Send(new GetInstallmentTemplatesByClubQuery(clubId,planId));
            return result.Match(
                (templates) => Ok(templates),
                Problem);
        }

        [HttpGet("/api/v{version:ApiVersion}/membership-plans/{planId:guid}/installment-templates")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<TemplatesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("ListInstallmentTemplatesByPlan")]
        [EndpointSummary("Retrieves active installment templates assigned to a membership plan.")]
        public async Task<IActionResult> ListByPlan(Guid planId)
        {
            var result = await sender.Send(new GetInstallmentTemplatesByClubQuery(null, planId));
            return result.Match(
                (templates) => Ok(templates),
                Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId}/installment-templates")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<InstallmentTemplateAdminsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("ListInstallmentTemplatesForManagement")]
        [EndpointSummary("List InstallmentTemplates for admins")]
        public async Task<IActionResult> ListForManagement( Guid clubId, [FromQuery] Guid? planId, [FromQuery]bool? active)
        {
            var result = await sender.Send(new GetTemplatesForManagementQuery(clubId, planId,active));
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
        [EndpointSummary("Updates the list of installments by sending new percentages, the number of installments & due after days won't change.")]
        public async Task<IActionResult> UpdateList(Guid id, [FromBody] List<decimal> newPercentages)
        {
            var result = await sender.Send(new UpdateInstallmentListCommand(id, newPercentages));
            return result.Match( _ => NoContent()
                , Problem);
        }
    }
}
