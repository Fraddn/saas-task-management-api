using ProjectSaas.Worker.EventHandlers;
using ProjectSaas.Worker.Models;
using ProjectSaas.Worker.Services.Interfaces;

namespace ProjectSaas.Worker.Services;

public sealed class OutboxEventDispatcher : IOutboxEventDispatcher
{
  private readonly IReadOnlyDictionary<string, IOutboxEventHandler> _handlers;

  public OutboxEventDispatcher(IEnumerable<IOutboxEventHandler> handlers)
  {
    _handlers = handlers.ToDictionary(
        handler => handler.EventType,
        handler => handler,
        StringComparer.OrdinalIgnoreCase);
  }

  public Task DispatchAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
  {
    if (!_handlers.TryGetValue(message.EventType, out var handler))
    {
      throw new InvalidOperationException(
          $"No outbox handler is registered for event type '{message.EventType}'.");
    }

    return handler.HandleAsync(message, cancellationToken);
  }
}