using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.RemoveAnswer
{
    public record RemoveAnswerCommand(
    Guid ApplicationId,
    Guid FieldDefinitionId,
    int InstanceIndex = 0
) : IRequest<Result<Success>>;
}
