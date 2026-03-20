using Microsoft.AspNetCore.SignalR;
using ProjectSaas.Api.Application.Notifications;
using ProjectSaas.Api.Hubs;
using ProjectSaas.Api.Realtime;

namespace ProjectSaas.Api.Infrastructure.Realtime;

public sealed class SignalRNotificationRealtimeNotifier : INotificationRealtimeNotifier
{
  private readonly IHubContext<NotificationsHub> _hubContext;

  public SignalRNotificationRealtimeNotifier(IHubContext<NotificationsHub> hubContext)
  {
    _hubContext = hubContext;
  }

  public async Task NotifyRecipientAsync(NotificationLiveDto notification, CancellationToken ct)
  {
    var groupName = NotificationGroupNameBuilder.ForRecipient(
        notification.OrganisationId,
        notification.UserId);

    await _hubContext.Clients
        .Group(groupName)
        .SendAsync("notificationReceived", new NotificationDto
        {
          Id = notification.Id,
          Type = notification.Type,
          Title = notification.Title,
          Message = notification.Message,
          IsRead = notification.IsRead,
          CreatedAtUtc = notification.CreatedAtUtc,
          RelatedEntityId = notification.RelatedEntityId,
          RelatedEntityType = notification.RelatedEntityType
        }, ct);
  }
}