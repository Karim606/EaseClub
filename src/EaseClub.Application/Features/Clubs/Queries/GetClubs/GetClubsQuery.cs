using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetClubs
{
    public record GetClubsQuery(CursorPaginationParameters parameters) : IRequest<Result<CursorPaginatedResult<ClubsDto>>>;

    public class ClubsDto
    {
        public ClubsDto(Guid id, string name) { 
            Id = id; 
            Name = name;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
