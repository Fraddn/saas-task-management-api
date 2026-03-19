using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.Services.Interfaces;

public interface IOutboxEventDispatcher
{
  Task DispatchAsync(OutboxMessageRecord message, CancellationToken cancellationToken);
}