using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Application.Features.Events.Services;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EaseClub.Application.Features.Events.Commands.RegisterForEvent;

public class RegisterForEventCommandHandler(
    EventRegistrationService registrationService,
    IUnitOfWork unitOfWork,
    ILogger<RegisterForEventCommandHandler> logger)
    : IRequestHandler<RegisterForEventCommand, Result<EventActionResponseDto>>
{
    public async Task<Result<EventActionResponseDto>> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
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
            logger.LogWarning("Registration failed for event {EventId}: {Errors}",
                request.EventId, string.Join(", ", registrationResult.Errors.Select(e => e.Description)));
            return registrationResult.Errors.First();
        }

        var registration = registrationResult.Value;

        var pricingResult = await registrationService.CalculatePricingAsync(@event, isMember, registration, cancellationToken);

        var discount = registration.TotalBasePrice - pricingResult.TotalPrice;
        var appliedPolicies = string.Join(", ", pricingResult.AppliedPolicies.Select(p => p.Name));

        registration.ApplyPricing(discount, appliedPolicies);

        logger.LogInformation("Pricing calculated for registration {RegistrationId}: Base {Base}, Final {Final}",
            registration.Id, pricingResult.BasePrice, pricingResult.TotalPrice);

        if (registration.FinalTotal == 0)
        {
            var confirmResult = @event.ConfirmRegistration(registration.Id);
            if (confirmResult.IsError)
            {
                logger.LogError("Failed to auto-confirm free registration {Id}: {Error}", registration.Id, confirmResult.TopError.Description);
                return confirmResult.TopError;
            }
            logger.LogInformation("Auto-confirmed free registration {RegistrationId} for event {EventId}", registration.Id, @event.Id);
        }

        await registrationService.UpdateEventAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(registration.Id, @event.Name);
    }
}
