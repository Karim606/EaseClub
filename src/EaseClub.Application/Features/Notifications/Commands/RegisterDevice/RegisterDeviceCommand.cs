using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common.Results;
using MediatR;

namespace EaseClub.Application.Features.Notifications.Commands.RegisterDevice
{
    public record RegisterDeviceCommand( Guid UserId, string FcmToken, string DeviceID) : IRequest<Result<Success>>;

}
