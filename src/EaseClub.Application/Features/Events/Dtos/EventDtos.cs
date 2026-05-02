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
    Audience Audience,
    EventStatus Status,
    string Venue,
    string? ImageUrl,
    string? Badge,
    bool IsFeatured,
    List<TicketTypeDto> TicketTypes);

public record TicketTypeDto(
    Guid Id,
    string Name,
    string Description,
    AttendeeCategory Category,
    decimal Price,
    int Quantity,
    int? MaxPerMember);

public record EventSummaryDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    EventStatus Status,
    string Venue,
    string? ImageUrl,
    string? Badge,
    bool IsFeatured,
    int RegistrationsCount);

public record EventRegistrationDto(
    Guid Id,
    Guid EventId,
    Guid RegistrantId,
    RegistrationStatus Status,
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
