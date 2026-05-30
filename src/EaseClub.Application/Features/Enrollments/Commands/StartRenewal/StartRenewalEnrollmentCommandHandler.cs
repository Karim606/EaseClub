using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Enrollments.Services;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Enrollments.Commands.StartRenewal
{
    public class StartRenewalEnrollmentCommandHandler(
         ICurrentUserService currentUserService,
         IMembershipRepository membershipRepository,
         IMembershipPlanRepository planRepository,
         IInstallmentsTemplatesRepository installmentTemplatesRepository,
         EnrollmentManager enrollmentManager,
         ILogger<StartRenewalEnrollmentCommandHandler> logger)
         : IRequestHandler<StartRenewalEnrollmentCommand, Result<EnrollmentPaymentResponse>>
    {
        public async Task<Result<EnrollmentPaymentResponse>> Handle(StartRenewalEnrollmentCommand request, CancellationToken ct)
        {
            var membership = await membershipRepository.GetByIdAsync(request.MembershipId, ct);
            if (membership == null) return Error.NotFound(description: "Membership not found.");

            var plan = await planRepository.GetPlanWithDetailsAsync(membership.MembershipPlanId, ct);
            if (plan == null) return Error.NotFound(description: "Plan not found.");

            InstallmentTemplate? template = null;
            if (request.InstallmentTemplateId.HasValue)
            {
                template = await installmentTemplatesRepository.GetByIdAsync(request.InstallmentTemplateId.Value, ct);
                if (template == null) return Error.NotFound();

               var supportsTemplate = plan.SupportsTemplate(template.Id);
               if (!supportsTemplate) return Error.Conflict(description: "Plan does not support the selected template.");
            }

            var result = await enrollmentManager.CreatePayableEnrollmentAsync(
                membership.MemberId,
                membership.ClubId,
                plan,
                template,
                null,
                membership,
                ct);

            return result;
        }
    }
}
