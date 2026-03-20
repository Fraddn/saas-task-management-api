using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectSaas.Worker.Configuration;
using ProjectSaas.Worker.Contracts.Notifications;
using ProjectSaas.Worker.Services.Interfaces;

namespace ProjectSaas.Worker.Services;

public sealed class RealtimeNotificationDispatcher : IRealtimeNotificationDispatcher
{
  private readonly HttpClient _httpClient;
  private readonly RealtimeDeliveryOptions _options;
  private readonly ILogger<RealtimeNotificationDispatcher> _logger;

  public RealtimeNotificationDispatcher(
      HttpClient httpClient,
      IOptions<RealtimeDeliveryOptions> options,
      ILogger<RealtimeNotificationDispatcher> logger)
  {
    _httpClient = httpClient;
    _options = options.Value;
    _logger = logger;
  }

  public async Task DispatchAsync(Guid notificationId, CancellationToken ct)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, "internal/notifications/live");
    request.Headers.Add("X-Internal-Api-Key", _options.InternalApiKey);
    request.Content = JsonContent.Create(new NotifyNotificationCreatedRequest
    {
      NotificationId = notificationId
    });

    var response = await _httpClient.SendAsync(request, ct);

    if (!response.IsSuccessStatusCode)
    {
      var body = await response.Content.ReadAsStringAsync(ct);

      _logger.LogWarning(
          "Failed to dispatch live notification for NotificationId {NotificationId}. StatusCode: {StatusCode}. Body: {Body}",
          notificationId,
          (int)response.StatusCode,
          body);
    }
  }
}