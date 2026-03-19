namespace ProjectSaas.Api.Application.Notifications;

public interface INotificationService
{
  Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
      int page,
      int pageSize,
      CancellationToken ct);

  Task MarkAsReadAsync(Guid notificationId, CancellationToken ct);

  Task<int> GetUnreadCountAsync(CancellationToken ct);

  Task MarkAllAsReadAsync(CancellationToken ct);
}