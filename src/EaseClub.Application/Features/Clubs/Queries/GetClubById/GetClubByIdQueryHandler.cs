using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetClubById
{
    public class GetClubByIdQueryHandler(IClubRepository clubRepository,ILogger<GetClubByIdQueryHandler>logger)
        : IRequestHandler<GetClubByIdQuery, Result<ClubResponse>>
    {
        public async Task<Result<ClubResponse>> Handle(GetClubByIdQuery request, CancellationToken cancellationToken)
        {
            var club = await clubRepository.GetByIdAsync(request.Id);
            if (club == null)
            {
                logger.LogWarning("Club with Id {ClubId} not found", request.Id);
                return Error.NotFound(description: $"Club with Id {request.Id} not found");
            }
            var response = new ClubResponse
            {
                Id = club.Id,
                Name = club.Name
            };
            return response;
        }
    }
}
