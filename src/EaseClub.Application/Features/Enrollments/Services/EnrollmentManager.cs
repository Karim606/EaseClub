using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Repositories;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Enrollments.Services
{
    public class EnrollmentManager(
        IEnrollmentRepository enrollmentRepository,
        IMembershipRepository membershipRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<EnrollmentManager> logger)
    {
        public async Task<Result<EnrollmentPaymentResponse>> CreatePayableEnrollmentAsync(
            Guid userId,
            Guid clubId,
            MembershipPlan plan,
            InstallmentTemplate? template,
            MembershipApplication? application = null,
            Membership? existingMembership = null,
            CancellationToken ct = default)
        {
            // 1. Business Rule: One Active Enrollment Intent at a time
            var existingActive = await GetExistingActiveEnrollmentAsync(userId, plan.Id, application?.Id, existingMembership?.Id, ct);
            if (existingActive != null)
            {
                logger.LogInformation("Returning existing active enrollment {EnrollmentId} for user {UserId}", existingActive.Id, userId);
                return new EnrollmentPaymentResponse(existingActive.Id, existingActive.FirstInvoiceId ?? Guid.Empty, existingActive.Amount);
            }

            // 2. Business Rule: Already a Member? (Prevent duplicate joining)
            if (application == null && existingMembership == null) // This is a DirectPay flow
            {
                var currentMembership = await membershipRepository.GetByMemberAndTypeAsync(userId, plan.MembershipTypeId, ct);
                if (currentMembership != null)
                {
                    return Error.Conflict("AlreadyMember", $"You already have an active membership for {plan.MembershipTypeId}. Please use renewal instead.");
                }
            }

            // 3. Create Enrollment Entity
            Result<Enrollment> enrollmentResult;
            if (application != null)
                enrollmentResult = Enrollment.CreateFromApprovedApplication(application, plan);
            else if (existingMembership != null)
                enrollmentResult = Enrollment.CreateForRenewal(existingMembership, plan, template);
            else
                enrollmentResult = Enrollment.CreateForDirectPay(userId, clubId, plan, template);

            if (enrollmentResult.IsError) return enrollmentResult.TopError;

            var enrollment = enrollmentResult.Value;

            // 4. Create Invoice for the first installment
            var invoiceResult = Invoice.Create(enrollment, clubId, userId, enrollment.Amount);
            if (invoiceResult.IsError) return invoiceResult.TopError;

            var invoice = invoiceResult.Value;
            enrollment.AttachFirstInvoice(invoice.Id);

            // 5. Persist
            await enrollmentRepository.AddAsync(enrollment, ct);
            await invoiceRepository.AddAsync(invoice, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return new EnrollmentPaymentResponse(enrollment.Id, invoice.Id, enrollment.Amount);
        }

        private async Task<Enrollment?> GetExistingActiveEnrollmentAsync(
            Guid userId, 
            Guid planId, 
            Guid? appId, 
            Guid? membershipId, 
            CancellationToken ct)
        {
            if (appId.HasValue)
                return await enrollmentRepository.GetActiveByApplicationIdAsync(appId.Value, ct);
            
            if (membershipId.HasValue)
                return await enrollmentRepository.GetActiveByMembershipIdAsync(membershipId.Value, ct);

            return await enrollmentRepository.GetActiveDirectPayAsync(userId, planId, ct);
        }
    }
}
