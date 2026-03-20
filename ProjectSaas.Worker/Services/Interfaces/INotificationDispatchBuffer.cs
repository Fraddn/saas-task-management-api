namespace ProjectSaas.Worker.Services.Interfaces;

public interface INotificationDispatchBuffer
{
  void Add(Guid notificationId);
  IReadOnlyCollection<Guid> GetAll();
  void Clear();
}