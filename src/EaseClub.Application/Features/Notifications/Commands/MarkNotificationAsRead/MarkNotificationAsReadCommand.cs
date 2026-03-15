using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<Result<Success>>;

}
