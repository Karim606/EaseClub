using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries.GetClubEventStatusCounts
{
    public record GetClubEventStatusCountsQuery(Guid ClubId) : IRequest<Result<ClubEventStatusCountsDto>>;
}
