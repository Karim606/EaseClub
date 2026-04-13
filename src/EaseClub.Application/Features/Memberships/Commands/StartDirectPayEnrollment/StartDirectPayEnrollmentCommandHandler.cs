using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Memberships.Commands.StartDirectPayEnrollment
{
    public class StartDirectPayEnrollmentCommandHandler(
        ICurrentUserService currentUserService,
        IMembershipPlanRepository membershipPlanRepository,
        IMembershipTypeRepository membershipTypeRepository,
        IInstallmentsTemplatesRepository installmentTemplatesRepository,
        IInvoiceRepository invoiceRepository,
        ILogger<StartDirectPayEnrollmentCommandHandler> logger,
        IUnitOfWork unitOfWork)
        : IRequestHandler<StartDirectPayEnrollmentCommand, Result<EnrollmentPaymentResponse>>
    {
        public async Task<Result<EnrollmentPaymentResponse>> Handle(StartDirectPayEnrollmentCommand request, CancellationToken ct)
        {
            if (!Guid.TryParse(currentUserService.GetId(), out var userId))
                return Error.Unauthorized(description: "Invalid user ID");

            var membershipType = await membershipTypeRepository.GetByIdAsync(request.MembershipTypeId, ct);
            if (membershipType == null)
                return Error.NotFound(description: "Membership type not found.");

            var plan = await membershipPlanRepository.GetPlanWithDetailsAsync(request.MembershipPlanId);
            if (plan == null)
                return Error.NotFound(description: "plan not found");

            if (plan.MembershipTypeId != membershipType.Id)
                return Error.Conflict(description: "MembershipPlan isnt associated with this MembershipType");

            InstallmentTemplate? template = null;
            if (request.InstallmentTemplateId.HasValue)
            {
                template = await installmentTemplatesRepository.GetByIdAsync(request.InstallmentTemplateId.Value, ct);
                if (template == null)
                    return Error.NotFound(description: "Installment Template not found");

                if (!plan.SupportsTemplate(template.Id))
                    return Error.Conflict(description: "InstallmentTemplate isnt associated with this MembershipPlan");
            }

            var pendingEnrollmentResult = PendingEnrollment.CreateForDirectPay(userId, request.ClubId, plan, template);
            if (pendingEnrollmentResult.IsError)
                return pendingEnrollmentResult.TopError;

            var pendingEnrollment = pendingEnrollmentResult.Value;
            var invoiceResult = Invoice.Create(
               pendingEnrollment,
               pendingEnrollment.ClubId,
               pendingEnrollment.UserId,
               pendingEnrollment.Amount);

            if (invoiceResult.IsError)
            {
                logger.LogError("Failed to create invoice for pending enrollment {PendingEnrollmentId}: {Errors}", pendingEnrollment.Id, invoiceResult.Errors);
                return invoiceResult.TopError;
            }

            var invoice = invoiceResult.Value;
            var attachResult = pendingEnrollment.AttachFirstInvoice(invoice.Id);
            if (attachResult.IsError)
            {
                logger.LogError("Failed to attach invoice {InvoiceId} to pending enrollment {PendingEnrollmentId}: {Errors}", invoice.Id, pendingEnrollment.Id, attachResult.Errors);
                return attachResult.TopError;
            }

            await invoiceRepository.AddAsync(invoice, ct);

            await unitOfWork.SaveChangesAsync(ct);

            return new EnrollmentPaymentResponse(pendingEnrollment.Id,invoice.Id);
            
        }
    }
}
