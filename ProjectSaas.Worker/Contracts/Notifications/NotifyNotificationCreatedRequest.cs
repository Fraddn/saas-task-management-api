namespace ProjectSaas.Worker.Contracts.Notifications;

public sealed class NotifyNotificationCreatedRequest
{
  public Guid NotificationId { get; set; }
}