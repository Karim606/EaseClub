using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetTypeById
{
    public record GetMembershipTypeByIdQuery(Guid MembershipTypeId) : IRequest<Result<MembershipTypeDetails>>;

 
}
