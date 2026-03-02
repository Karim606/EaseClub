using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.SubmitApplication
{
    public record SubmitApplicationCommand(Guid ApplicationId) : IRequest<Result<Success>>;
}
