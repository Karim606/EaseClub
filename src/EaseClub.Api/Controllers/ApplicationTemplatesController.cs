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


        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId}/application-templates")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(OffsetPaginatedResult<TemplateSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetTemplates")]
        [EndpointSummary("Retrieves paginated application templates for a specific club.")]
        public async Task<IActionResult> GetTemplates([FromRoute] Guid clubId, [FromQuery] OffsetPaginationParameters parameters,
            CancellationToken ct)
        {
            var result = await sender.Send(new GetApplicationTemplatesQuery(clubId, parameters), ct);
            return result.Match(
                items => Ok(items),
                Problem);
        }

        [HttpGet("{templateId:Guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(TemplateTreeQuery), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [EndpointName("GetTemplateById")]
        [EndpointSummary("Retrieves a full application template including steps, sections, and fields.")]
        #region swagger-description
        [EndpointDescription(@"
Retrieves a full Application Template tree used for dynamic form generation.

STRUCTURE OVERVIEW:

1. Template
   - Root entity containing the entire form definition.

2. Steps
   - Ordered execution stages of the form.
   - Each step defines a logical grouping of sections.
   - Example: Personal Info → Documents → Payment Info.

3. Sections
   - Logical UI blocks inside a step.
   - Each section has:
     • Intent (General | FamilyMembers)
     • RepeatRule (controls repetition behavior)
     • Fields (actual input definitions)

   INTENT OPTIONS:
   - General: Standard form section
   - FamilyMembers: Section is repeated per family member instance

4. RepeatRule
   Controls how many times a section can appear.

   MODE OPTIONS:
   - ExactValue:
       The section MUST appear exactly N times.
   - AtLeastOne:
       The section must appear at least once and up to N times.

   EXAMPLES:
   - FamilyMembers + AtLeastOne + 3 → user can add 1–3 family members
   - ExactValue + 1 → section is fixed single instance

5. Fields
   - Individual inputs inside a section.
   - Each field includes:
     • FieldType (Text, Number, Date, etc.)
     • ValidationRuleSet
     • AllowedValues (for dropdown-like inputs)
     • System flags (isSystemField)

6. ValidationRuleSet
   Defines constraints applied to user input:

   TEXT RULES:
   - IsRequired → field must be filled
   - MinLength / MaxLength → character limits

   NUMERIC RULES:
   - MinValue / MaxValue → numeric boundaries

   DATE RULES:
   - MinDate → earliest allowed date
   - MaxDate → latest allowed date

   VALIDATION BEHAVIOR:
   - All rules are evaluated together
   - Any violation returns structured validation errors
   - Empty value is allowed only if IsRequired = false

USAGE:
This endpoint is used to dynamically render multi-step application forms with repeatable sections and fully validated fields.
")]
        #endregion
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
        #region swagger-description
        [EndpointDescription(@"
Creates or updates an Application Template Definition used to dynamically generate multi-step application forms.

========================================================
OVERVIEW
========================================================

A Template defines the full structure of an application form including:

1. Steps (top-level workflow stages)
2. Sections (logical form groups inside steps)
3. Fields (individual inputs inside sections)
4. Validation rules (input constraints)
5. Repeat rules (dynamic section repetition)
6. System section rules (special behaviors like FamilyMembers)

========================================================
STEP STRUCTURE
========================================================

Each Step represents a logical phase of the application:

Examples:
- Personal Information
- Documents Upload
- Payment Details

Steps are:
- Ordered (execution sequence matters)
- Container for Sections only
- Not directly user input

========================================================
SECTION STRUCTURE
========================================================

Sections define grouped UI blocks inside a Step.

Each Section contains:

- Title → Display name
- Intent → Behavior type
- RepeatRule → How many instances allowed
- Fields → Input definitions

--------------------------------------------------------
SECTION INTENT OPTIONS
--------------------------------------------------------

1. General
   - Standard section
   - Used for single-instance data collection

2. FamilyMembers
   - Special system section
   - Represents repeatable entities (e.g., dependents, family members)
   - Must align with MembershipPlan rules if used

--------------------------------------------------------
REPEAT RULE (IMPORTANT)
--------------------------------------------------------

Controls how many times a section can appear.

Mode Options:

1. ExactValue
   - Section must appear exactly N times
   - Strict fixed structure

2. AtLeastOne
   - Section must appear 1 to N times
   - Used for dynamic collections (e.g., family members)

Example:
- FamilyMembers + AtLeastOne + 3 → user can add 1–3 members

========================================================
FIELDS STRUCTURE
========================================================

Fields represent actual user inputs.

Each field includes:

- Key → unique identifier
- Label → UI label
- FieldType → Text, Number, Date, Enum, etc.
- ValidationRules → constraints
- AllowedValues(nullable) → dropdown options (if applicable) used if type fi field is Enum.
- SystemField flag → prevents deletion if required by system

--------------------------------------------------------
FIELD VALIDATION RULES
--------------------------------------------------------

ValidationRuleSet defines constraints:

TEXT:
- IsRequired → mandatory field
- MinLength / MaxLength → string limits

NUMERIC:
- MinValue / MaxValue → numeric boundaries

DATE:
- MinDate / MaxDate → allowed date range

RULE BEHAVIOR:
- All rules are evaluated together
- Any violation produces validation errors
- Empty values are allowed only if IsRequired = false

========================================================
SYSTEM CONSTRAINTS
========================================================

- Field keys must be unique across entire template
- System fields cannot be removed
- FamilyMembers section is required if any connected MembershipPlan supports family members
- Template must remain structurally consistent after updates

========================================================
USAGE
========================================================

This endpoint is used to:
- Build dynamic multi-step application forms
- Drive frontend UI generation
- Enforce validation rules server-side
- Support repeatable sections (e.g., family members)
")]

        #endregion
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
