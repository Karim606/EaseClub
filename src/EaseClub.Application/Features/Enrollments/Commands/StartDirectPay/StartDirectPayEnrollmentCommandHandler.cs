using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Enrollments.Services;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Enrollments.Commands.StartDirectPay
{
    public class StartDirectPayEnrollmentCommandHandler(
        ICurrentUserService currentUserService,
        IMembershipPlanRepository membershipPlanRepository,
        IMemberUserRepository memberUserRepository,
        IMembershipTypeRepository membershipTypeRepository,
        IInstallmentsTemplatesRepository installmentTemplatesRepository,
        ILogger<StartDirectPayEnrollmentCommandHandler> logger,
        EnrollmentManager enrollmentManager)
        : IRequestHandler<StartDirectPayEnrollmentCommand, Result<EnrollmentPaymentResponse>>
    {
        public async Task<Result<EnrollmentPaymentResponse>> Handle(StartDirectPayEnrollmentCommand request, CancellationToken ct)
        {
            var memberUser = await memberUserRepository.GetByIdAsync(request.MemberId, ct);
            if (memberUser == null) return Error.NotFound(description: "Member not found.");

            var membershipType = await membershipTypeRepository.GetByIdAsync(request.MembershipTypeId, ct);
            if (membershipType == null) return Error.NotFound(description: "Membership type not found.");

            var plan = await membershipPlanRepository.GetPlanWithDetailsAsync(request.MembershipPlanId);
            if (plan == null) return Error.NotFound(description: "plan not found");
            
            if (plan.MembershipTypeId != membershipType.Id) 
                return Error.Conflict(description: "MembershipPlan isnt associated with this MembershipType");

            InstallmentTemplate? template = null;
            if (request.InstallmentTemplateId.HasValue)
            {
                template = await installmentTemplatesRepository.GetByIdAsync(request.InstallmentTemplateId.Value, ct);
                if (template == null) return Error.NotFound(description: "Installment Template not found");
                
                if (!plan.SupportsTemplate(template.Id))
                    return Error.Conflict(description: "InstallmentTemplate isnt associated with this MembershipPlan");
            }

            var result = await enrollmentManager.CreatePayableEnrollmentAsync(
                request.MemberId,
                request.ClubId,
                plan,
                template,
                null,
                null,
                ct);

            return result;
        }
    }
}
