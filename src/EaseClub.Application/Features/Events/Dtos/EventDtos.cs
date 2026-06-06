using EaseClub.Domain.Events.Enums;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Events.Dtos;

public record EventDto(
    Guid Id,
    Guid ClubId,
    string ClubName,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    int RemainingCapacity,
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
    bool RequiresMembership);

public record EventSummaryDto(
    Guid Id,
    Guid ClubId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    EventAccessType AccessType,
    EventStatus Status,
    string Venue,
    string ClubName,
    string? ImageUrl,
    string? Badge,
    int Capacity,
    int RemainingCapacity,
    int RegistrationsCount);

public record EventRegistrationDto(
    Guid Id,
    Guid EventId,
    Guid ClubId,
    Guid RegistrantId,
    bool IsRegistrantAttending,
    RegistrationStatus Status,
    Guid? InvoiceId,
    decimal TotalBasePrice,
    decimal DiscountAmount,
    decimal FinalTotal,
    string? AppliedPolicies,
    string ReadableId,
    // Event context for client display (avoids extra API calls)
    string EventName,
    DateTime EventStartDate,
    string EventVenue,
    string? EventImageUrl,
    string EventClubName,
    List<AttendeeDto> Attendees);

public record AttendeeDto(
    Guid Id,
    Guid TicketTypeId,
    AttendeeCategory TicketCategory,
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

public record EventRegistrationPreviewDto(
    Guid EventId,
    decimal TotalBasePrice,
    decimal DiscountAmount,
    decimal FinalTotal,
    List<AppliedPolicyPreviewDto> AppliedPolicies,
    List<TicketBreakdownDto> TicketBreakdown);

public record AppliedPolicyPreviewDto(
    string Name,
    decimal Adjustment);

public record TicketBreakdownDto(
    Guid TicketTypeId,
    AttendeeCategory TicketCategory,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

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
