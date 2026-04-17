using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep
{
    public class CompleteStepHandler(
        IMembershipApplicationRepository appRepo,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<CompleteStepCommand, Result<StepProgressResponse>>
    {
        public async Task<Result<StepProgressResponse>> Handle(CompleteStepCommand request, CancellationToken ct)
        {
            var app = await appRepo.GetByIdAsync(request.ApplicationId, ct);
            if (app == null) return Error.NotFound("Application not found");

            // Map DTOs to Domain Value Objects
            var domainAnswers = request.Answers.Select(dto =>
                ApplicationAnswer.Create(app.Id, dto.FieldId, dto.Key, dto.FieldType, dto.Value, dto.InstanceIndex).Value
            ).ToList();

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
