using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.AssignPolicy
{
    public class AssignPoliciesCommandHandler(
        IPricingPolicyRepository policyRepo,
        IApplicationTemplateRepository templateRepo,
        IEventRepository eventRepo,
        IUnitOfWork unitOfWork,
        ILogger<AssignPoliciesCommandHandler> logger)
    : IRequestHandler<AssignPoliciesCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            AssignPoliciesCommand request,
            CancellationToken ct)
        {

            var policies = await policyRepo.GetPoliciesByIdAsync(request.Policies.Select(x => x.PolicyId).ToList());

            if (policies.Count != request.Policies.Count) { logger.LogError("NotFound error in AssignPoliciesCommandHandler: {Error}", Error.NotFound("Policies.SomeNotFound", "One or more pricing policies were not found.").ToLogObject()); return Error.NotFound("Policies.SomeNotFound", "One or more pricing policies were not found."); }

            // 3. Assign
            switch (request.TargetType)
            {
                case PricingPolicyTargetType.ApplicationTemplate:
                    var template = await templateRepo.GetFullTemplateAsync(request.TargetId);
                    if (template == null) { logger.LogError("NotFound error in AssignPoliciesCommandHandler: {Error}", Error.NotFound("Template.NotFound").ToLogObject()); return Error.NotFound("Template.NotFound"); }
            foreach (var policyRequest in request.Policies)
                    {
                        var policy = policies.First(p => p.Id == policyRequest.PolicyId);

                        // Let the Domain Entity handle the business rules (like priority conflicts)
                        var result = template.AssignPolicy(policy, policyRequest.Priority);

                        if (result.IsError) { logger.LogError("Error in AssignPoliciesCommandHandler: {Error}", result.TopError.ToLogObject()); return result.TopError; }// Fail fast if a domain rule is broken
                    }
                    break;

                case PricingPolicyTargetType.Event:
                    var @event = await eventRepo.GetByIdAsync(request.TargetId);
                    if (@event == null) { logger.LogError("NotFound error in AssignPoliciesCommandHandler: {Error}", Error.NotFound("Event.NotFound").ToLogObject()); return Error.NotFound("Event.NotFound"); }

                    foreach (var policyRequest in request.Policies)
                    {
                        var policy = policies.First(p => p.Id == policyRequest.PolicyId);

                        // Update the Event's PricingPolicyIds list
                        var addResult = @event.AssignPricingPolicy(policy.Id);
                        if (addResult.IsError) { logger.LogError("Error in AssignPoliciesCommandHandler: {Error}", addResult.TopError.ToLogObject()); return addResult.TopError; }

                        // Create the PricingPolicyAssignment row for the generic assignments table
                        var assignmentResult = PricingPolicyAssignment.Create(
                            @event.ClubId,
                            policy.Id,
                            @event.Id,
                            policyRequest.Priority,
                            PricingPolicyTargetType.Event,
                            policy,
                            EventFieldKeys.All
                        );
                        if (assignmentResult.IsError) { logger.LogError("Error in AssignPoliciesCommandHandler: {Error}", assignmentResult.TopError.ToLogObject()); return assignmentResult.TopError; }

                        await policyRepo.AddAssignmentAsync(assignmentResult.Value);
                    }
                    break;

                default:
                   return Error.Validation("InvalidTargetType");
                   
            }

            // 4. Save
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
