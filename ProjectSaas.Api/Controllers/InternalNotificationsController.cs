using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectSaas.Api.Application.Notifications;
using ProjectSaas.Api.Configuration;
using ProjectSaas.Api.Contracts.Notifications;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("internal/notifications")]
public sealed class InternalNotificationsController : ControllerBase
{
  private const string ApiKeyHeaderName = "X-Internal-Api-Key";

  private readonly InternalCallbackOptions _options;
  private readonly INotificationLiveQueryService _liveQueryService;
  private readonly INotificationRealtimeNotifier _realtimeNotifier;

  public InternalNotificationsController(
      IOptions<InternalCallbackOptions> options,
      INotificationLiveQueryService liveQueryService,
      INotificationRealtimeNotifier realtimeNotifier)
  {
    _options = options.Value;
    _liveQueryService = liveQueryService;
    _realtimeNotifier = realtimeNotifier;
  }

  [HttpPost("live")]
  public async Task<IActionResult> PushLiveNotification(
      [FromBody] NotifyNotificationCreatedRequest request,
      CancellationToken ct)
  {
    if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
        string.IsNullOrWhiteSpace(_options.ApiKey) ||
        !string.Equals(providedKey.ToString(), _options.ApiKey, StringComparison.Ordinal))
    {
      return Unauthorized();
    }

    var notification = await _liveQueryService.GetByIdAsync(request.NotificationId, ct);

    if (notification is null)
    {
      return NotFound();
    }

    await _realtimeNotifier.NotifyRecipientAsync(notification, ct);

    return Accepted();
  }
}