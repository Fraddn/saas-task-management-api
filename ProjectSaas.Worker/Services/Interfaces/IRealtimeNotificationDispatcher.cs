namespace ProjectSaas.Worker.Services.Interfaces;

public interface IRealtimeNotificationDispatcher
{
  Task DispatchAsync(Guid notificationId, CancellationToken ct);
}