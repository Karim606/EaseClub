using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Errors;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep
{
    public class CompleteStepHandler(IMembershipApplicationRepository appRepo,
        IUnitOfWork unitOfWork,
        ILogger<CompleteStepHandler> logger) : IRequestHandler<CompleteStepCommand, Result<StepProgressResponse>>
    {
        public async Task<Result<StepProgressResponse>> Handle(CompleteStepCommand request, CancellationToken ct)
        {
            var app = await appRepo.GetByIdAsync(request.ApplicationId, ct);
            if (app == null) { logger.LogError("NotFound error in CompleteStepHandler: {Error}", Error.NotFound("Application not found").ToLogObject()); return Error.NotFound("Application not found"); }

            // 1. Get all fields defined in the current step from the backend snapshot
            var stepFields = app.TemplateSnapshot.Steps
                .FirstOrDefault(s => s.Order == request.StepOrder)?
                .Sections.SelectMany(s => s.Fields)
                .ToDictionary(f => f.Id);

            if (stepFields == null) return MembershipApplicationErrors.StepNotFound;

            var domainAnswers = new List<EaseClub.Domain.MembershipApplications.ValueObjects.UserAnswer>();
            var mappingErrors = new List<Error>();

            // 2. Map & Enrich: Only use the FieldId and Value from the client
            foreach (var dto in request.Answers)
            {
                // Zero Trust: If the field isn't in this step's snapshot, ignore or error
                if (!stepFields.TryGetValue(dto.FieldId, out var fieldDefinition))
                    continue;

                // Set technical properties (Key, Type) from the BACKEND definition
                domainAnswers.Add(new EaseClub.Domain.MembershipApplications.ValueObjects.UserAnswer(
                    dto.FieldId,
                    fieldDefinition.Key,
                    dto.Value,
                    dto.InstanceId,
                    fieldDefinition.FieldType
                ));
            }
            if (mappingErrors.Any()) return mappingErrors;

            var result = app.CompleteStep(request.StepOrder, domainAnswers);
            if (result.IsError) return (result.Errors.ToList());

            await unitOfWork.SaveChangesAsync(ct);

            var response = new StepProgressResponse(
                app.CurrentStepOrder,
                app.CompletedStepOrders.ToList()
            );

            return response;
        }
    }
}
