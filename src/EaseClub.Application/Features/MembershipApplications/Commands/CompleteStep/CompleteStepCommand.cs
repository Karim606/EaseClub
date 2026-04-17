using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.CompleteStep
{
    public record CompleteStepCommand(
     Guid ApplicationId,
     int StepOrder,
     List<AnswerDto> Answers) : IRequest<Result<StepProgressResponse>>;

    public record StepProgressResponse(
        int CurrentStepOrder,
        List<int> CompletedStepOrders
    );
}
