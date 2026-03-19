using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.EventHandlers;

public interface IOutboxEventHandler
{
  string EventType { get; }

  Task HandleAsync(OutboxMessageRecord message, CancellationToken cancellationToken);
}