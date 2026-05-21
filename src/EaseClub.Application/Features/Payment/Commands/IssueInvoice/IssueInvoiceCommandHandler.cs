using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Payment;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.IssueInvoice
{
    public class IssueInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPendingEnrollmentRepository pendingEnrollmentRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ILogger<IssueInvoiceCommandHandler> logger)
        : IRequestHandler<IssueInvoiceCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(IssueInvoiceCommand request, CancellationToken ct)
        {
            IBillingItem? billingItem = null;
            Guid memberId = Guid.Empty;
            Guid clubId = Guid.Empty;

            switch (request.Type)
            {
                case BillingItemType.PendingEnrollmentFirstInstallment:
                    var pending = await pendingEnrollmentRepository.GetByIdAsync(request.BillingItemId, ct);
                    if (pending == null)
                    {
                        logger.LogError("Pending enrollment {Id} not found", request.BillingItemId);
                        return Error.NotFound("Pending enrollment not found");
                    }

                    if (pending.FirstInvoiceId.HasValue)
                        return pending.FirstInvoiceId.Value;

                    billingItem = pending;
                    memberId = pending.UserId;
                    clubId = pending.ClubId;
                    break;

                case BillingItemType.MembershipInstallment:
                    var membership = await membershipRepository.GetByInstallmentIdAsync(request.BillingItemId, ct);
                    if (membership == null)
                    {
                        logger.LogError("Membership for installment {Id} not found", request.BillingItemId);
                        return Error.NotFound("Installment not found");
                    }

                    var installment = membership.MembershipCycles
                        .SelectMany(c => c.Installments)
                        .FirstOrDefault(i => i.Id == request.BillingItemId);

                    if (installment == null)
                    {
                        logger.LogError("Installment {Id} not found in membership {MembershipId}", request.BillingItemId, membership.Id);
                        return Error.NotFound("Installment not found");
                    }

                    if (installment.InvoiceId.HasValue)
                        return installment.InvoiceId.Value;

                    billingItem = installment;
                    memberId = membership.MemberId;
                    clubId = membership.ClubId;
                    break;

                default:
                    return Error.Validation("Unsupported billing item type");
            }

            var invoiceResult = Invoice.Create(billingItem, clubId, memberId, billingItem.Amount);
            if (invoiceResult.IsError)
            {
                logger.LogError("Failed to create invoice for {Type} {Id}: {Errors}", request.Type, request.BillingItemId, invoiceResult.Errors);
                return invoiceResult.TopError;
            }

            var invoice = invoiceResult.Value;

            // Link back to source
            if (request.Type == BillingItemType.PendingEnrollmentFirstInstallment)
            {
                var attachResult = ((PendingEnrollment)billingItem).AttachFirstInvoice(invoice.Id);
                if (attachResult.IsError) return attachResult.TopError;
            }
            else if (request.Type == BillingItemType.MembershipInstallment)
            {
                var attachResult = ((MembershipInstallment)billingItem).AttachInvoice(invoice.Id);
                if (attachResult.IsError) return attachResult.TopError;
            }

            await invoiceRepository.AddAsync(invoice, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return invoice.Id;
        }
    }
}
