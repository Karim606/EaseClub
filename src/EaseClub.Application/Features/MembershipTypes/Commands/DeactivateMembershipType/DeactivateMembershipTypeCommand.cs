using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.DeactivateMembershipType
{
    public record DeactivateMembershipTypeCommand(Guid MembershipTypeId):IRequest<Result<Success>>;
}
