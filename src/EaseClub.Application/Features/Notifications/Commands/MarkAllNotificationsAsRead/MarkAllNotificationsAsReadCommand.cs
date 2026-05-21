using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead
{
    public record MarkAllNotificationsAsReadCommand(Guid? UserId,Guid? ClubId) : IRequest<Result<Success>>;
}
