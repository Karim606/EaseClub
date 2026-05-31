using EaseClub.Application.Features.Events.Commands.AddTicketType;
using EaseClub.Application.Features.Events.Commands.CancelEvent;
using EaseClub.Application.Features.Events.Commands.CancelRegistration;
using EaseClub.Application.Features.Events.Commands.CreateEvent;
using EaseClub.Application.Features.Events.Commands.PublishEvent;
using EaseClub.Application.Features.Events.Commands.PreviewEventRegistration;
using EaseClub.Application.Features.Events.Commands.RegisterForEvent;
using EaseClub.Application.Features.Events.Commands.RemoveTicketType;
using EaseClub.Application.Features.Events.Commands.UpdateEvent;
using EaseClub.Application.Features.Events.Commands.UpdateTicketType;
using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Application.Features.Events.Queries;
using EaseClub.Application.Features.Events.Queries.GetUpcomingEvents;
using EaseClub.Application.Features.Events.Queries.GetMyRegistrations;
using EaseClub.Application.Features.Events.Queries.GetRegistrationById;
using EaseClub.Application.Features.Events.Queries.GetEventRegistrations;
using EaseClub.Application.Features.Events.Queries.GetFamilyMembersForEvent;
using EaseClub.Application.Features.Events.Queries.GetEventStats;
using EaseClub.Application.Features.Events.Queries.GetClubEventStatusCounts;
using EaseClub.Application.Common.Interfaces; 
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Events.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Api.Controllers;

[Route("api/v{version:ApiVersion}/events")]
public class EventsController(ISender sender) : ApiController
{
    // ─── Queries ────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetEventById")]
    [EndpointSummary("Get a single event with full details including ticket types")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await sender.Send(new GetEventByIdQuery(id));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("club/{clubId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UnifiedPaginatedResponse<EventSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetClubEvents")]
    [EndpointSummary("Get all events for a specific club (all statuses) with pagination, search and status filter")]
    public async Task<IActionResult> GetByClubId(
        Guid clubId, 
        [FromQuery] string? search, 
        [FromQuery] EventStatus? status, 
        [FromQuery] PaginationRequest pagination)
    {
        var result = await sender.Send(new GetClubEventsQuery(clubId, search, status, pagination));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("upcoming")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UnifiedPaginatedResponse<EventSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetGlobalUpcomingEvents")]
    [EndpointSummary("Get all upcoming published events with pagination, search, and optional eligibility filtering")]
    public async Task<IActionResult> GetGlobalUpcoming(
        [FromQuery] bool eligibleOnly,
        [FromQuery] string? search, 
        [FromQuery] PaginationRequest pagination,
        [FromServices] ICurrentUserService currentUserService)
    {
        Guid? userId = null;
        if (Guid.TryParse(currentUserService.GetId(), out var parsedId))
        {
            userId = parsedId;
        }

        var result = await sender.Send(new GetUpcomingEventsQuery(
            ClubId: null,
            MemberId: userId,
            EligibleOnly: eligibleOnly,
            Search: search,
            Pagination: pagination));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("club/{clubId:guid}/upcoming")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UnifiedPaginatedResponse<EventSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetUpcomingEvents")]
    [EndpointSummary("Get upcoming published events for a club with pagination, search, and optional eligibility filtering")]
    public async Task<IActionResult> GetUpcoming(
        Guid clubId, 
        [FromQuery] bool eligibleOnly,
        [FromQuery] string? search, 
        [FromQuery] PaginationRequest pagination,
        [FromServices] ICurrentUserService currentUserService)
    {
        Guid? userId = null;
        if (Guid.TryParse(currentUserService.GetId(), out var parsedId))
        {
            userId = parsedId;
        }

        var result = await sender.Send(new GetUpcomingEventsQuery(
            ClubId: clubId, 
            MemberId: userId,
            EligibleOnly: eligibleOnly,
            Search: search, 
            Pagination: pagination));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("club/{clubId:guid}/status-counts")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ClubEventStatusCountsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetClubEventsStatusCounts")]
    [EndpointSummary("Admin: Get the counts of total, published, draft, and cancelled events for a club")]
    public async Task<IActionResult> GetStatusCounts(Guid clubId)
    {
        var result = await sender.Send(new GetClubEventStatusCountsQuery(clubId));
        return result.Match(counts => Ok(counts), Problem);
    }

    [HttpGet("my-registrations")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(List<EventRegistrationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetMyRegistrations")]
    [EndpointSummary("Get all event registrations created by the currently authenticated user")]
    public async Task<IActionResult> GetMyRegistrations([FromServices] ICurrentUserService currentUserService)
    {
        var userId = Guid.Parse(currentUserService.GetId() ?? Guid.Empty.ToString());
        var result = await sender.Send(new GetMyRegistrationsQuery(userId));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("registrations/{regId:guid}")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventRegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetRegistrationById")]
    [EndpointSummary("Get details of a specific registration including all attendees")]
    public async Task<IActionResult> GetRegistrationById(Guid regId)
    {
        var result = await sender.Send(new GetRegistrationByIdQuery(regId));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("{id:guid}/registrations")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UnifiedPaginatedResponse<EventRegistrationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetEventRegistrations")]
    [EndpointSummary("Admin: Get all registrations for a specific event with pagination")]
    public async Task<IActionResult> GetEventRegistrations(Guid id, [FromQuery] PaginationRequest pagination)
    {
        var result = await sender.Send(new GetEventRegistrationsQuery(id, pagination));
        return result.Match(_ => Ok(_), Problem);
    }

    [HttpGet("{id:guid}/stats")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointName("GetEventStats")]
    [EndpointSummary("Admin: Retrieve ticket sales and capacity stats for an event")]
    public async Task<IActionResult> GetStats(Guid id)
    {
        var result = await sender.Send(new GetEventStatsQuery(id));
        return result.Match(stats => Ok(stats), Problem);
    }

    [HttpGet("club/{clubId:guid}/family-members")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(List<FamilyMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("GetFamilyMembersForEvent")]
    [EndpointSummary("Get the current user's family members eligible for registration in a club's events")]
    public async Task<IActionResult> GetFamilyMembersForEvent(Guid clubId, [FromServices] ICurrentUserService currentUserService)
    {
        var userId = Guid.Parse(currentUserService.GetId() ?? Guid.Empty.ToString());
        var result = await sender.Send(new GetFamilyMembersForEventQuery(userId, clubId));
        return result.Match(_ => Ok(_), Problem);
    }

    // ─── Commands ───────────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("CreateEvent")]
    [EndpointSummary("Admin: Create a new event draft for a club")]
    public async Task<IActionResult> Create(CreateEventCommand command)
    {
        var result = await sender.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetById), new { version = "1.0", id = response.Id }, response),
            Problem);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("UpdateEvent")]
    [EndpointSummary("Admin: Update an event's details (only allowed in Draft status)")]
    public async Task<IActionResult> Update(Guid id, UpdateEventCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch.");
        var result = await sender.Send(command);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{id:guid}/tickets")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("AddTicketType")]
    [EndpointSummary("Admin: Add a ticket type to a draft event")]
    public async Task<IActionResult> AddTicket(Guid id, AddTicketTypeCommand command)
    {
        if (id != command.EventId) return BadRequest("ID mismatch.");
        var result = await sender.Send(command);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{id:guid}/tickets/{ticketId:guid}")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("UpdateTicketType")]
    [EndpointSummary("Admin: Update an existing ticket type on a draft event")]
    public async Task<IActionResult> UpdateTicket(Guid id, Guid ticketId, UpdateTicketTypeCommand command)
    {
        if (id != command.EventId || ticketId != command.TicketTypeId) return BadRequest("ID mismatch.");
        var result = await sender.Send(command);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpDelete("{id:guid}/tickets/{ticketId:guid}")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("RemoveTicketType")]
    [EndpointSummary("Admin: Remove a ticket type from a draft event")]
    public async Task<IActionResult> RemoveTicket(Guid id, Guid ticketId)
    {
        var result = await sender.Send(new RemoveTicketTypeCommand(id, ticketId));
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("PublishEvent")]
    [EndpointSummary("Admin: Publish a draft event to make it visible and open for registration")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await sender.Send(new PublishEventCommand(id));
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "ClubAdmin,SuperAdmin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("CancelEvent")]
    [EndpointSummary("Admin: Cancel an event and notify all registered members")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await sender.Send(new CancelEventCommand(id));
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{id:guid}/register")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("RegisterForEvent")]
    [EndpointSummary("Register the current user (and attendees) for a published event")]
    public async Task<IActionResult> Register(Guid id, RegisterForEventCommand command, [FromServices] ICurrentUserService currentUserService)
    {
        if (id != command.EventId) return BadRequest("ID mismatch.");
        // Override RegistrantId from the authenticated token — never trust the request body for identity.
        var registrantId = Guid.Parse(currentUserService.GetId() ?? Guid.Empty.ToString());
        var secureCommand = command with { RegistrantId = registrantId };
        var result = await sender.Send(secureCommand);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{id:guid}/registration-preview")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventRegistrationPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("PreviewEventRegistration")]
    [EndpointSummary("Preview event registration attendees, ticket prices, applied policies, and final total")]
    public async Task<IActionResult> PreviewRegistration(Guid id, PreviewEventRegistrationCommand command, [FromServices] ICurrentUserService currentUserService)
    {
        if (id != command.EventId) return BadRequest("ID mismatch.");
        var registrantId = Guid.Parse(currentUserService.GetId() ?? Guid.Empty.ToString());
        var secureCommand = command with { RegistrantId = registrantId };
        var result = await sender.Send(secureCommand);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{id:guid}/registrations/{regId:guid}/cancel")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(EventActionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointName("CancelRegistration")]
    [EndpointSummary("Cancel an existing registration and release the reserved tickets")]
    public async Task<IActionResult> CancelRegistration(Guid id, Guid regId)
    {
        var result = await sender.Send(new CancelRegistrationCommand(id, regId));
        return result.Match(response => Ok(response), Problem);
    }
}
