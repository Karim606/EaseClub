using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.PricingPolices;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication
{
    public class CreateApplicationCommandHandler(
    IApplicationTemplateRepository tempRepo,
    IMembershipPlanRepository membershipPlanRepo,
    IMembershipTypeRepository membershipTyeRepo,
    IMembershipApplicationRepository appRepo,
    IPricingPolicyRepository pricingPolicyRepo,
    ICurrentUserService currentUserService,
    IInstallmentsTemplatesRepository installmentsTemplatesRepository,
    ILogger<CreateApplicationCommandHandler>logger,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateApplicationCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateApplicationCommand request, CancellationToken ct)
        {
            // 1. Fetch Live Template
            var resOfParse = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!resOfParse) { logger.LogError("Unauthorized error in CreateApplicationCommandHandler: {Error}", Error.Unauthorized(description: "Invalid user ID").ToLogObject()); return Error.Unauthorized(description: "Invalid user ID"); }

            // 1.2 Fetch Membership Plan
            var plan = await membershipPlanRepo.GetPlanWithDetailsAsync(request.MembershipPlanId);

            if (plan == null) { logger.LogError("NotFound error in CreateApplicationCommandHandler: {Error}", Error.NotFound(description: "plan not found").ToLogObject()); return Error.NotFound(description: "plan not found"); }
            var template = await tempRepo.GetFullTemplateAsync(plan.ApplicationTemplateId!.Value, ct);
            if (template == null) { logger.LogError("NotFound error in CreateApplicationCommandHandler: {Error}", Error.NotFound("Template not found").ToLogObject()); return Error.NotFound("Template not found"); }

            // 1.1 Fetch Membership Type
            var memType = await membershipTyeRepo.GetByIdAsync(request.MembershipTypeId);
            if (memType == null) { logger.LogError("NotFound error in CreateApplicationCommandHandler: {Error}", Error.NotFound(description: "Membership type not found.").ToLogObject()); return Error.NotFound(description: "Membership type not found."); }

            // 1.3 Validate Enrollment Mode
            if (plan.EnrollmentMode != EnrollmentMode.ApplicationForm) { logger.LogError("Conflict error in CreateApplicationCommandHandler: {Error}", Error.Conflict(description: "Plan WrongEnrollmentMode").ToLogObject()); return Error.Conflict(description: "Plan WrongEnrollmentMode"); }

            // 2. Fetch Installments Template if exists
            List<Installment> installments = new List<Installment>();
            InstallmentTemplate? installmentTemplate = null;

            if (request.InstallmentTemplateId == null)
            {
                installments.Add(Installment.Create(100m, 0, 1).Value);

            }

            else
            {
                 installmentTemplate = await installmentsTemplatesRepository.GetByIdAsync(request.InstallmentTemplateId.Value);

                if (installmentTemplate == null) { logger.LogError("NotFound error in CreateApplicationCommandHandler: {Error}", Error.NotFound(description: "Installment Template not found").ToLogObject()); return Error.NotFound(description: "Installment Template not found"); }

                installments = installmentTemplate.Installments.ToList();
            }

            // 4. Fetch Live Pricing Policies for this club
            var policyAssignments = await pricingPolicyRepo.GetPricingPolicyAssignmentsByTargetIdAsync(template.Id, ct);
            var policies = await pricingPolicyRepo.GetPoliciesByIdAsync(policyAssignments.Select(a => a.PolicyId).ToList(), ct);

            // 3. Create the Frozen Snapshot

            List<PricingPolicySnapshot> policySnapshots = new List<PricingPolicySnapshot>();
            policies =policies.Where(p => p.IsActive).ToList();

            foreach (var assignment in policyAssignments)
            {
                var policy = policies.FirstOrDefault(p => p.Id == assignment.PolicyId);
                if (policy != null)
                    policySnapshots.Add(policy.ToSnapshot(assignment.Priority));
            }
            var snapshot = template.ToSnapshot(plan.TotalPrice,
                policySnapshots,
                plan.ToSnapshot(),
                Installment.ListToSnapshot(installments)
                );

          
            // 4. Initialize the Aggregate
            var trackingNumber = $"APP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var application = MembershipApplication.Create(
                Guid.NewGuid(),
                trackingNumber,
                snapshot,
                userId,
                request.ClubId,
                plan,
                installmentTemplate,
                memType,
                plan.ApplicationTemplateId!.Value).Value;

            await appRepo.AddAsync(application);
            await unitOfWork.SaveChangesAsync(ct);

            return application.Id;
        }
    }
}
