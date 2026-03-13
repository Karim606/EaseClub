using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.DeleteTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.SyncMembershipTypes;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
using EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplateById;
using EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/admin/application-templates")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")] // Restrict to authorized staff
    public class ApplicationTemplatesController(ISender sender) : ApiController
    {


        #region Template Shell


        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(OffsetPaginatedResult<TemplateTreeQuery>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetTemplates")]
        [EndpointSummary("Retrieves paginated application templates for a specific club.")]
        public async Task<IActionResult> GetTemplates([FromQuery] Guid clubId, [FromQuery] OffsetPaginationParameters parameters,
            CancellationToken ct)
        {
            var result = await sender.Send(new GetApplicationTemplatesQuery(clubId, parameters), ct);
            return result.Match(
                items => Ok(items),
                Problem);
        }

        [HttpGet("{templateId:Guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(TemplateDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetTemplateById")]
        [EndpointSummary("Retrieves a full application template including steps, sections, and fields.")]
        public async Task<IActionResult> GetTemplate(Guid templateId,
            CancellationToken ct)
        {
            var result = await sender.Send(new GetTemplateByIdQuery(templateId), ct);
            return result.Match(
                temp => Ok(temp),
                Problem);
        }

        [HttpPost("upsert")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("UpsertTemplate")]
        [EndpointSummary("Creates a new template or updates an existing one including steps, sections, and fields.")]
        public async Task<IActionResult> UpsertTemplate([FromBody] UpsertTemplateCommand request, CancellationToken ct)
        {
            var result = await sender.Send(request, ct);

            return result.Match(
                    _ => NoContent(),
                    Problem);
        }

        [HttpDelete("{templateId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("DeleteTemplate")]
        [EndpointSummary("Deletes an application template and all its associated steps, sections, and fields.")]
        public async Task<IActionResult> DeleteTemplate(Guid templateId, CancellationToken ct)
        {
            // Note: Use UserContext to get ClubId if not in the route
            var result = await sender.Send(new DeleteTemplateCommand(templateId), ct);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPut("{templateId}/membership-plans")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("SyncTemplateMembershipPlans")]
        [EndpointSummary("Synchronizes the membership plans connected to a template by replacing the existing list with the provided membership plan IDs.")]
        public async Task<IActionResult> SyncMembershipPlans(
        Guid templateId,
        [FromBody] SyncTemplateMembershipPlansCommand command,
        CancellationToken ct)
        {
            var result = await sender.Send(command with { TemplateId = templateId }, ct);

            return result.Match(
                _ => NoContent(),
                Problem);
        }

        #endregion
    }

    #region old patches system
    //[HttpPost]
    //public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command, CancellationToken ct)
    //{

    //    var result = await sender.Send(command, ct);
    //    return result.Match(
    //        (id) => Ok(id),
    //        Problem);
    //}
    //[HttpGet("{templateId}/steps/by-order/{order}")]
    //public async Task<IActionResult> GetStepByOrder(Guid templateId, int order,CancellationToken ct)
    //{
    //    // ClubId is retrieved from the secure context or query param as discussed
    //    var result = await sender.Send(new GetTemplateStepByOrderQuery(templateId, order),ct);

    //    return result.Match( items => Ok(items), Problem);
    //}

    //[HttpPut("{templateId}")]
    //public async Task<IActionResult> UpdateTemplate(Guid templateId, [FromBody] UpdateTemplateCommand command,CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { TemplateId = templateId },ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //[HttpGet("{id}")]
    //public async Task<IActionResult> GetTemplateStructure(Guid id)
    //{
    //    var result = await _mediator.Send(new GetTemplateStructureQuery(id));
    //    return result == null ? NotFound() : Ok(result);
    //}


    //#region Steps

    //[HttpPost("{templateId}/steps")]
    //public async Task<IActionResult> AddStep(Guid templateId, [FromBody] AddStepCommand command, CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { TemplateId = templateId }, ct);
    //    return result.Match(id => Ok(id), Problem);
    //}

    //[HttpPut("steps/{stepId}")]
    //public async Task<IActionResult> UpdateStep(Guid stepId, [FromBody] UpdateStepCommand command,CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { StepId = stepId },ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //#endregion

    //#region Sections
    //[HttpPost("steps/{stepId}/sections")]
    //public async Task<IActionResult> AddSection([FromRoute]Guid stepId, [FromBody] AddSectionCommand command,CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { StepId = stepId },ct);

    //    return result.Match(
    //        id => Ok(id),
    //        Problem);
    //}

    //[HttpPut("sections/{sectionId}")]
    //public async Task<IActionResult> UpdateSection(Guid sectionId, [FromBody] UpdateSectionCommand command,CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { SectionId = sectionId },ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //#endregion

    //#region Fields
    //[HttpPost("sections/{sectionId}/fields")]
    //public async Task<IActionResult> AddField(Guid sectionId, [FromBody] AddFieldCommand command,CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { SectionId = sectionId },ct);

    //    return result.Match(
    //        id => Ok(id),
    //        Problem);
    //}

    //[HttpPut("fields/{fieldId}")]
    //public async Task<IActionResult> UpdateField(Guid fieldId, [FromBody] UpdateFieldCommand command,CancellationToken ct)
    //{
    //    var result = await sender.Send(command with { FieldId = fieldId },ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //#endregion

    //#region Remove Operations

    //[HttpDelete("steps/{stepId}")]
    //public async Task<IActionResult> RemoveStep(Guid stepId, [FromQuery] Guid templateId, [FromHeader(Name = "X-Club-Id")] Guid clubId, CancellationToken ct)
    //{
    //    var result = await sender.Send(new RemoveStepCommand(clubId, templateId, stepId), ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //[HttpDelete("steps/{stepId}/sections/{sectionId}")]
    //public async Task<IActionResult> RemoveSection(Guid stepId, Guid sectionId, [FromHeader(Name = "X-Club-Id")] Guid clubId, CancellationToken ct)
    //{
    //    var result = await sender.Send(new RemoveSectionCommand(clubId, stepId, sectionId), ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //[HttpDelete("sections/{sectionId}/fields/{fieldId}")]
    //public async Task<IActionResult> RemoveField(Guid sectionId, Guid fieldId, [FromQuery] Guid templateId, [FromHeader(Name = "X-Club-Id")] Guid clubId, CancellationToken ct)
    //{
    //    var result = await sender.Send(new RemoveFieldCommand(clubId, templateId, sectionId, fieldId), ct);
    //    return result.Match(_ => NoContent(), Problem);
    //}

    //#endregion
    //#endregion

    //#region Management Actions

    //[HttpPut("{id}/activate")]
    //public async Task<IActionResult> ActivateTemplate(Guid id)
    //{
    //    var result = await _mediator.Send(new ActivateTemplateCommand(id));
    //    return result.IsError ? BadRequest(result.TopError) : NoContent();
    //}

    //#endregion

    #endregion
}
