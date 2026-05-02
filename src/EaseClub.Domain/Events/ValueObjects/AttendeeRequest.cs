using System;

namespace EaseClub.Domain.Events.ValueObjects;

public record AttendeeRequest(Guid TicketTypeId, Guid? AttendeeId, string AttendeeName, int? Age = null, string? Gender = null);
