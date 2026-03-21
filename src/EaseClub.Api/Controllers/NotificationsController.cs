using EaseClub.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using EaseClub.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using EaseClub.Application.Features.Notifications.Commands.RegisterDevice;
using EaseClub.Application.Features.Notifications.Queries.GetNotifications;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/notifications")]
    public class NotificationsController(ISender sender) : ApiController
    {

        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [EndpointName("GetNotifications")]
        [EndpointSummary("Lists notifications based on filter criteria.")]
        [EndpointDescription("Returns a list of notifications that match the specified filter criteria.")]
        public async Task<IActionResult> Get([FromQuery]GetNotificationsQuery query,CancellationToken ct)
        {
            var result = await sender.Send(query,ct);
            return result.Match(
                notifications => Ok(notifications),
                Problem);
        }


        [HttpPatch("{notificationId:guid}/read")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [EndpointName("MarkNotificationAsRead")]
        [EndpointSummary("Marks a specific notification as read.")]
        [EndpointDescription("Marks a single notification as read by its ID.")]
        public async Task<IActionResult> MarkRead([FromRoute] Guid notificationId, CancellationToken ct)
        {
            var command = new MarkNotificationAsReadCommand(notificationId);
            var result = await sender.Send(command, ct);
            return result.Match(
                _ => NoContent(),
                Problem
            );
        }


        [HttpPatch("read-all")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [EndpointName("MarkAllNotificationsAsRead")]
        [EndpointSummary("Marks all notifications as read for the specified user or club.")]
        [EndpointDescription("Marks all unread notifications as read. Either UserId or ClubId must be provided.")]
        public async Task<IActionResult> MarkAllRead([FromBody] MarkAllNotificationsAsReadCommand command, CancellationToken ct)
        {
            var result = await sender.Send(command, ct);
            return result.Match(
                _ => NoContent(),  
                Problem
            );
        }

        [HttpPost("devices")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [EndpointName("RegisterDevice")]
        [EndpointSummary("Registers or updates a user's device token for push notifications.")]
        [EndpointDescription("Sends FCM token, device ID, and user ID to backend for notification dispatching.")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceCommand command, CancellationToken ct)
        {
            var result = await sender.Send(command, ct);
            return result.Match(
                _ => NoContent(),
                Problem
            );
        }

    }
}
