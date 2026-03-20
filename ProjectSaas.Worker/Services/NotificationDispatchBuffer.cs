using ProjectSaas.Worker.Services.Interfaces;

namespace ProjectSaas.Worker.Services;

public sealed class NotificationDispatchBuffer : INotificationDispatchBuffer
{
  private readonly List<Guid> _notificationIds = new();

  public void Add(Guid notificationId)
  {
    _notificationIds.Add(notificationId);
  }

  public IReadOnlyCollection<Guid> GetAll()
  {
    return _notificationIds.AsReadOnly();
  }

  public void Clear()
  {
    _notificationIds.Clear();
  }
}