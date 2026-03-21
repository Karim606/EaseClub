using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Commands.RegisterDevice
{
    public class RegisterDeviceCommandHandler(
        IDeviceRepository deviceRepository) : IRequestHandler<RegisterDeviceCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
        {

            await  deviceRepository.UpsertDeviceAsync(new DeviceDto(request.UserId,request.DeviceID, request.FcmToken, DateTime.UtcNow ),cancellationToken);
            return Result.Success;
        }
    }
}
