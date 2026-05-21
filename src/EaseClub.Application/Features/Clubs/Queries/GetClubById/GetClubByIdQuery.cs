using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetClubById
{
    public record GetClubByIdQuery(Guid Id):IRequest<Domain.Common.Results.Result<ClubResponse>>;
    
}
