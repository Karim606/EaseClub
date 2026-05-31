using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Application.Features.Events.Services;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Events.Commands.PreviewEventRegistration;

public class PreviewEventRegistrationCommandHandler(
    EventRegistrationService registrationService,
    ILogger<PreviewEventRegistrationCommandHandler> logger)
    : IRequestHandler<PreviewEventRegistrationCommand, Result<EventRegistrationPreviewDto>>
{
    public async Task<Result<EventRegistrationPreviewDto>> Handle(PreviewEventRegistrationCommand request, CancellationToken cancellationToken)
    {
        var loadResult = await registrationService.LoadEventAndResolveRegistrantAsync(
            request.EventId, request.RegistrantId, request.RegistrantName, cancellationToken);

        if (loadResult.IsError)
            return loadResult.Errors.First();

        var (@event, isMember, registrantName, familyMembers) = loadResult.Value;

        var attendees = registrationService.NormalizeAttendees(
            request.Attendees ?? [], familyMembers, request.RegistrantId, registrantName);

        var familyMemberIds = familyMembers.Select(f => f.Id).ToList();

        var registrationResult = @event.Register(
            request.RegistrantId,
            isMember,
            request.IsRegistrantAttending,
            registrantName,
            request.RegistrantAge,
            request.RegistrantGender,
            attendees,
            familyMemberIds);

        if (registrationResult.IsError)
        {
            logger.LogWarning("Registration preview failed for event {EventId}: {Errors}",
                request.EventId, string.Join(", ", registrationResult.Errors.Select(e => e.Description)));
            return registrationResult.Errors.First();
        }

        var registration = registrationResult.Value;
        var pricingResult = await registrationService.CalculatePricingAsync(@event, isMember, registration, cancellationToken);
        var discountAmount = registration.TotalBasePrice - pricingResult.TotalPrice;

        return new EventRegistrationPreviewDto(
            @event.Id,
            registration.TotalBasePrice,
            discountAmount,
            pricingResult.TotalPrice,
            pricingResult.AppliedPolicies.Select(p => new AppliedPolicyPreviewDto(p.Name, p.Adjustment)).ToList(),
            EventRegistrationService.BuildTicketBreakdown(@event, registration));
    }
}
