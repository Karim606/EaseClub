using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
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
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignPoliciesCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            AssignPoliciesCommand request,
            CancellationToken ct)
        {

            var policies = await policyRepo.GetPoliciesByIdAsync(request.Policies.Select(x => x.PolicyId).ToList());

            if (policies.Count != request.Policies.Count)
                return Error.NotFound("Policies.SomeNotFound", "One or more pricing policies were not found.");

            // 3. Assign
            switch (request.TargetType)
            {
                case PricingPolicyTargetType.ApplicationTemplate:
                    var template = await templateRepo.GetFullTemplateAsync(request.TargetId);
                    if (template == null) return Error.NotFound("Template.NotFound");
                    foreach (var policyRequest in request.Policies)
                    {
                        var policy = policies.First(p => p.Id == policyRequest.PolicyId);

                        // Let the Domain Entity handle the business rules (like priority conflicts)
                        var result = template.AssignPolicy(policy, policyRequest.Priority);

                        if (result.IsError) return result; // Fail fast if a domain rule is broken
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
