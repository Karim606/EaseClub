using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Queries.GetNotifications
{
  public record GetNotificationsQuery (Guid? UserId,Guid? ClubId,bool? IsRead,PaginationRequest PaginationParameters):IRequest<Result<UnifiedPaginatedResponse<NotificationDto>>>;

 public record NotificationDto(Guid Id,Guid? UserId,Guid? ClubId,bool IsRead,string Title,string Message,DateTime CreatedAt);

    

}
