using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.Enums;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.PricingPolices;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static EaseClub.Domain.Memberships.MembershipStatus;
using EaseClub.Domain.Memberships;

namespace EaseClub.Application.Features.Events.Commands.RegisterForEvent;

public class RegisterForEventCommandHandler(
    IEventRepository eventRepository,
    IMembershipRepository membershipRepository,
    IPricingPolicyRepository pricingPolicyRepository,
    IUnitOfWork unitOfWork,
    ILogger<RegisterForEventCommandHandler> logger)
    : IRequestHandler<RegisterForEventCommand, Result<EventActionResponseDto>>
{
    public async Task<Result<EventActionResponseDto>> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
    {
        // 1. Load Event with details
        var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        // 2. Check if registrant has an Active membership in this club
        var memberships = await membershipRepository.GetByMemberIdAsync(request.RegistrantId,MembershipStatus.Active, cancellationToken);
        var activeMemberships = memberships.Where(m => m.ClubId == @event.ClubId).ToList();
        bool isMember = activeMemberships.Any();
        
        var allFamilyMembers = activeMemberships.SelectMany(m => m.FamilyMembers).ToList();
        var familyMemberIds = allFamilyMembers.Select(f => f.Id).ToList();

        var attendees = request.Attendees?.ToList() ?? new List<Domain.Events.ValueObjects.AttendeeRequest>();

        // Override age for family members to prevent spoofing
        for (int i = 0; i < attendees.Count; i++)
        {
            var req = attendees[i];
            if (req.AttendeeId.HasValue)
            {
                var fm = allFamilyMembers.FirstOrDefault(f => f.Id == req.AttendeeId.Value);
                if (fm != null)
                {
                    attendees[i] = req with { Age = fm.GetAge() };
                }
            }
        }

        // 3. Register in domain (handles capacity, ticket availability, age/gender restrictions inside domain)
        var registrationResult = @event.Register(
            request.RegistrantId,
            isMember,
            request.IsRegistrantAttending,
            request.RegistrantName,
            request.RegistrantAge,
            request.RegistrantGender,
            attendees,
            familyMemberIds);

        if (registrationResult.IsError)
        {
            logger.LogWarning("Registration failed for event {EventId}: {Errors}", 
                request.EventId, string.Join(", ", registrationResult.Errors.Select(e => e.Description)));
            return registrationResult.Errors.First();
        }

        var registration = registrationResult.Value;

        // 4. Pricing Logic
        if (@event.PricingPolicyIds.Any())
        {
            var attendeeCategories = registration.Attendees.Select(a => {
                var ticket = @event.TicketTypes.First(t => t.Id == a.TicketTypeId);
                return ticket.Category;
            }).ToList();

            var pricingData = new Dictionary<string, string?>
            {
                { "attendee_count", attendeeCategories.Count.ToString() },
                { "member_count", attendeeCategories.Count(c => c == AttendeeCategory.Member).ToString() },
                { "guest_count", attendeeCategories.Count(c => c == AttendeeCategory.Guest).ToString() },
                { "family_count", attendeeCategories.Count(c => c == AttendeeCategory.FamilyMember).ToString() },
                { "non_member_count", attendeeCategories.Count(c => c != AttendeeCategory.Member).ToString() },
                { "is_member", isMember.ToString().ToLower() }
            };

            var context = new PricingContext(pricingData);

            // Load policies
            var allClubPolicies = await pricingPolicyRepository.GetByClubIdAsync(@event.ClubId, cancellationToken);
            var assignedPolicies = allClubPolicies.Where(p => @event.PricingPolicyIds.Contains(p.Id)).ToList();

            if (assignedPolicies.Any())
            {
                var pricingResult = PricingEngine.Calculate(registration.TotalBasePrice, (IPricingPolicy[])assignedPolicies.ToArray(), context);
                
                var discount = registration.TotalBasePrice - pricingResult.TotalPrice;
                var appliedPolicies = string.Join(", ", pricingResult.AppliedPolicies.Select(p => p.Name));
                
                registration.ApplyPricing(discount, appliedPolicies);
                
                logger.LogInformation("Pricing calculated for registration {RegistrationId}: Base {Base}, Final {Final}", 
                    registration.Id, pricingResult.BasePrice, pricingResult.TotalPrice);
            }
        }

        // 5. Persistence
        // NOTE: Invoice is NOT created here. The mobile client calls IssueInvoice(registrationId)
        // which creates the real Invoice entity and links it back. This keeps registration
        // and billing concerns separated.
        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(registration.Id, @event.Name);
    }
}
