using EaseClub.Domain.Events;
using Microsoft.EntityFrameworkCore;
using EaseClub.Domain.Events.Entities;
namespace EaseClub.Infrastructure.Data.Repositories;

public class EventRepository : EfRepository<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Event?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.TicketTypes)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Event>> GetByClubIdAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.ClubId == clubId)
            .Include(e => e.TicketTypes)
            .Include(e => e.Registrations)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Event>> GetUpcomingEventsAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.ClubId == clubId && e.Status == EaseClub.Domain.Events.Enums.EventStatus.Published && e.StartDate > DateTime.UtcNow)
            .Include(e => e.TicketTypes)
            .Include(e => e.Registrations)
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<EventRegistration?> GetRegistrationByIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        var @event = await _context.Events
            .Include(e => e.TicketTypes)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Attendees)
            .FirstOrDefaultAsync(e => e.Registrations.Any(r => r.Id == registrationId), cancellationToken);

        return @event?.Registrations.FirstOrDefault(r => r.Id == registrationId);
    }

    public async Task<List<EventRegistration>> GetRegistrationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var events = await _context.Events
            .Include(e => e.TicketTypes)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Attendees)
            .Where(e => e.Registrations.Any(r => r.RegistrantId == userId))
            .ToListAsync(cancellationToken);

        return events.SelectMany(e => e.Registrations).Where(r => r.RegistrantId == userId).ToList();
    }

    public async Task<List<EventRegistration>> GetRegistrationsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var @event = await _context.Events
            .Include(e => e.TicketTypes)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        return @event?.Registrations.ToList() ?? new List<EventRegistration>();
    }
}
