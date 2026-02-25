using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication
{
    public record CreateApplicationCommand(
    Guid ClubId,
    Guid TemplateId,
    Guid MembershipTypeId,
    Guid MembershipPlanId) : IRequest<Result<Guid>>;
}
