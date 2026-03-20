namespace ProjectSaas.Api.Application.Notifications;

public interface INotificationRealtimeNotifier
{
  Task NotifyRecipientAsync(NotificationLiveDto notification, CancellationToken ct);
}