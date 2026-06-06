using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.Enums;
using EaseClub.Domain.Events.ValueObjects;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.PricingPolices;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Events.Services;

public class EventRegistrationService(
    IEventRepository eventRepository,
    IMembershipRepository membershipRepository,
    IPricingPolicyRepository pricingPolicyRepository,
    ILogger<EventRegistrationService> logger)
{
    public async Task<Result<(Event Event, bool IsMember, string RegistrantName, List<FamilyMember> FamilyMembers)>>
        LoadEventAndResolveRegistrantAsync(Guid eventId, Guid registrantId, string fallbackName, CancellationToken ct)
    {
        var @event = await eventRepository.GetWithDetailsAsync(eventId, ct);
        if (@event == null)
            return Error.NotFound("Event.NotFound", "Event not found.");

        var memberships = await membershipRepository.GetByMemberIdAsync(registrantId, MembershipStatus.Active, ct);
        var activeMemberships = memberships.Where(m => m.ClubId == @event.ClubId).ToList();
        var isMember = activeMemberships.Any();

        var member = activeMemberships.Select(m => m.Member).FirstOrDefault(m => m != null);
        var registrantName = member != null
            ? $"{member.FirstName} {member.LastName}".Trim()
            : fallbackName;

        var familyMembers = activeMemberships.SelectMany(m => m.FamilyMembers).ToList();

        return (@event, isMember, registrantName, familyMembers);
    }

    public List<AttendeeRequest> NormalizeAttendees(
        List<AttendeeRequest> attendees,
        List<FamilyMember> familyMembers,
        Guid registrantId,
        string registrantName)
    {
        var result = attendees.ToList();

        for (var i = 0; i < result.Count; i++)
        {
            var attendee = result[i];
            if (!attendee.AttendeeId.HasValue)
                continue;

            var familyMember = familyMembers.FirstOrDefault(f => f.Id == attendee.AttendeeId.Value);
            if (familyMember != null)
            {
                result[i] = attendee with { AttendeeName = familyMember.FullName, Age = familyMember.GetAge() };
                continue;
            }

            if (attendee.AttendeeId.Value == registrantId)
                result[i] = attendee with { AttendeeName = registrantName };
        }

        return result;
    }

    public async Task<PricingResult> CalculatePricingAsync(
        Event @event,
        bool isMember,
        Domain.Events.Entities.EventRegistration registration,
        CancellationToken ct)
    {
        var attendeeCategories = registration.Attendees
            .Select(a => @event.TicketTypes.First(t => t.Id == a.TicketTypeId).Category)
            .ToList();

        var pricingData = new Dictionary<string, string?>
        {
            { EventFieldKeys.AttendeeCount, attendeeCategories.Count.ToString() },
            { EventFieldKeys.GuestCount, attendeeCategories.Count(c => c == AttendeeCategory.Guest).ToString() },
            { EventFieldKeys.FamilyMemberCount, attendeeCategories.Count(c => c == AttendeeCategory.FamilyMember).ToString() },
            { EventFieldKeys.IsMember, isMember.ToString().ToLower() }
        };

        var assignments = await pricingPolicyRepository.GetPricingPolicyAssignmentsByTargetIdAsync(@event.Id, ct);
        var policySnapshots = assignments
            .Where(a => a.TargetType == PricingPolicyTargetType.Event)
            .OrderBy(a => a.Priority)
            .Select(a => a.Policy.ToSnapshot(a.Priority))
            .ToArray();

        return policySnapshots.Any()
            ? PricingEngine.Calculate(registration.TotalBasePrice, policySnapshots, new PricingContext(pricingData))
            : new PricingResult(registration.TotalBasePrice, registration.TotalBasePrice, []);
    }

    public async Task UpdateEventAsync(Event @event, CancellationToken ct)
    {
        await eventRepository.UpdateAsync(@event, ct);
    }

    public static List<TicketBreakdownDto> BuildTicketBreakdown(
        Event @event,
        Domain.Events.Entities.EventRegistration registration)
    {
        return registration.Attendees
            .GroupBy(a => a.TicketTypeId)
            .Select(group =>
            {
                var ticket = @event.TicketTypes.First(t => t.Id == group.Key);
                return new TicketBreakdownDto(
                    ticket.Id,
                    ticket.Category,
                    group.Count(),
                    ticket.BasePrice,
                    ticket.BasePrice * group.Count());
            })
            .ToList();
    }
}
