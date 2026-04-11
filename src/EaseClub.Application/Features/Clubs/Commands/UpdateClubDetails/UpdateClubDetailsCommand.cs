using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Commands.UpdateClubDetails
{
    public record UpdateClubDetailsCommand(
    Guid ClubId,
    UpdateClubDetailsRequest Data
) : IRequest<Result<Success>>,IRequireClubAdmin;

    public record UpdateClubDetailsRequest(
    string about,
    string Phone,
    string Email,
    List<WorkScheduleDto> WorkSchedules,
    List<string> Amenities,
    Guid? LogoId
);

    public record WorkScheduleDto(string Label, string TimeRange);
}
