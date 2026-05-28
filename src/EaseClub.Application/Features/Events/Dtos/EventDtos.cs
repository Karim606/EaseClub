using EaseClub.Domain.Events.Enums;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Events.Dtos;

public record EventDto(
    Guid Id,
    Guid ClubId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    EventAccessType AccessType,
    EventStatus Status,
    string Venue,
    string? ImageUrl,
    string? Badge,
    List<Guid> PricingPolicyIds,
    List<TicketTypeDto> TicketTypes);

public record TicketTypeDto(
    Guid Id,
    AttendeeCategory Category,
    decimal BasePrice,
    int TotalQuantity,
    int AvailableQuantity,
    int? MaxPerMember,
    bool RequiresMembership,
    int? MinAge,
    int? MaxAge,
    string? GenderRestriction);

public record EventSummaryDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    EventAccessType AccessType,
    EventStatus Status,
    string Venue,
    string? ImageUrl,
    string? Badge,
    int RemainingCapacity,
    int RegistrationsCount);

public record EventRegistrationDto(
    Guid Id,
    Guid EventId,
    Guid RegistrantId,
    bool IsRegistrantAttending,
    RegistrationStatus Status,
    Guid? InvoiceId,
    decimal TotalBasePrice,
    decimal DiscountAmount,
    decimal FinalTotal,
    string? AppliedPolicies,
    List<AttendeeDto> Attendees);

public record AttendeeDto(
    Guid Id,
    Guid TicketTypeId,
    Guid? AttendeeId,
    string AttendeeName,
    int? Age,
    string? Gender);

public record FamilyMemberDto(
    Guid Id,
    string FullName,
    EaseClub.Domain.Memberships.FamilyRelationship Relationship,
    int Age);

public record EventActionResponseDto(Guid Id, string Name, Guid? InvoiceId = null);

public record EventStatsDto(
    int TotalCapacity,
    int TotalSold,
    int TotalAvailable,
    Dictionary<string, int> Details);

public record ClubEventStatusCountsDto(
    int Total,
    int Published,
    int Draft,
    int Cancelled);
