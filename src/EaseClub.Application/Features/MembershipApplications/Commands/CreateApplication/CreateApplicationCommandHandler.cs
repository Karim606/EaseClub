using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
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
            var template = await tempRepo.GetFullTemplateAsync(request.TemplateId, ct);
            if (template == null) return Error.NotFound("Template not found");

            var memType = await membershipTyeRepo.GetByIdAsync(request.MembershipTypeId);
            if (memType == null)
                return Error.NotFound(description: "Membership type not found.");

            var plan = await  membershipPlanRepo.GetPlanWithDetailsAsync(request.MembershipPlanId);

            if (plan == null )
               return Error.NotFound(description: "plan not found");

            if(plan.EnrollmentMode != EnrollmentMode.ApplicationForm) 
                return Error.Conflict(description: "Plan WrongEnrollmentMode");
            // 2. Fetch Live Pricing Policies for this club
            var policies = await pricingPolicyRepo.GetByClubIdAsync(request.ClubId, ct);

            List<Installment> installments = new List<Installment>();
            InstallmentTemplate? installmentTemplate = null;

            if (request.InstallmentTemplateId == null)
            {
                installments.Add(Installment.Create(100, 0, 1).Value);

            }

            else
            {
                 installmentTemplate = await installmentsTemplatesRepository.GetByIdAsync(request.InstallmentTemplateId.Value);

                if (installmentTemplate == null)
                    return Error.NotFound(description: "Installment Template not found");

                installments = installmentTemplate.Installments.ToList();
            }
            // 3. Create the Frozen Snapshot
            var snapshot = template.ToSnapshot(plan.TotalPrice,
                policies.Select(p => p.ToSnapshot()).ToList(),
                plan.ToSnapshot(),
                Installment.ListToSnapshot(installments)
                );

            Guid.TryParse(currentUserService.GetId(), out var userId);
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
                request.TemplateId).Value;

            await appRepo.AddAsync(application);
            await unitOfWork.SaveChangesAsync(ct);

            return application.Id;
        }
    }
}
