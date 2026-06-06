using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Notifications.Queries.GetUnreadNotificationsCount
{
    public record GetUnreadNotificationsCountQuery(Guid? UserId, Guid? ClubId) : IRequest<Result<int>>;
}
