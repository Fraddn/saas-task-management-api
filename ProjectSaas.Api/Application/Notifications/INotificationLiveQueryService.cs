namespace ProjectSaas.Api.Application.Notifications;

public interface INotificationLiveQueryService
{
  Task<NotificationLiveDto?> GetByIdAsync(Guid notificationId, CancellationToken ct);
}