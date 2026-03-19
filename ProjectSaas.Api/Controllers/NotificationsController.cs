using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Notifications;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
  private readonly INotificationService _notificationService;

  public NotificationsController(INotificationService notificationService)
  {
    _notificationService = notificationService;
  }

  [HttpGet]
  [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetNotifications(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      CancellationToken ct = default)
  {
    var notifications = await _notificationService.GetUserNotificationsAsync(page, pageSize, ct);
    return Ok(notifications);
  }

  [HttpGet("unread-count")]
  [ProducesResponseType(typeof(UnreadNotificationCountDto), StatusCodes.Status200OK)]
  public async Task<ActionResult<UnreadNotificationCountDto>> GetUnreadCount(
      CancellationToken ct = default)
  {
    var count = await _notificationService.GetUnreadCountAsync(ct);

    return Ok(new UnreadNotificationCountDto
    {
      Count = count
    });
  }

  [HttpPatch("{notificationId:guid}/read")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> MarkAsRead(
      Guid notificationId,
      CancellationToken ct = default)
  {
    await _notificationService.MarkAsReadAsync(notificationId, ct);
    return NoContent();
  }

  [HttpPatch("read-all")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
  {
    await _notificationService.MarkAllAsReadAsync(ct);
    return NoContent();
  }
}