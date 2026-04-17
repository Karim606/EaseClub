using EaseClub.Domain.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Domain.Events;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Event>> GetByClubIdAsync(Guid clubId, CancellationToken cancellationToken = default);
    Task<List<Event>> GetUpcomingEventsAsync(Guid clubId, CancellationToken cancellationToken = default);
    Task<EaseClub.Domain.Events.Entities.EventRegistration?> GetRegistrationByIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<List<EaseClub.Domain.Events.Entities.EventRegistration>> GetRegistrationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<EaseClub.Domain.Events.Entities.EventRegistration>> GetRegistrationsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}
