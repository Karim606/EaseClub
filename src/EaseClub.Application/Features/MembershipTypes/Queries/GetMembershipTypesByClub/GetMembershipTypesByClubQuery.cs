using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesByClub
{
    public record GetMembershipTypesByClubQuery(Guid ClubId):IRequest<Result<List<MembershipTypeDto>>>;
    
}
