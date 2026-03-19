using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketHighPriorityHandler : IOutboxEventHandler
{
  private readonly ILogger<TicketHighPriorityHandler> _logger;

  public TicketHighPriorityHandler(ILogger<TicketHighPriorityHandler> logger)
  {
    _logger = logger;
  }

  public string EventType => "TicketHighPriority";

  public Task HandleAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
  {
    var payload = JsonSerializer.Deserialize<TicketHighPriorityEvent>(
        message.PayloadJson,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (payload is null)
    {
      throw new InvalidOperationException(
          $"Failed to deserialize TicketHighPriority payload for outbox message {message.Id}.");
    }

    _logger.LogInformation(
        "Handled {EventType} for TicketId {TicketId} in OrganisationId {OrganisationId}. Priority: {Priority}",
        message.EventType,
        payload.TicketId,
        payload.OrganisationId,
        payload.Priority);

    return Task.CompletedTask;
  }
}

internal sealed class TicketHighPriorityEvent
{
  public Guid TicketId { get; set; }
  public Guid OrganisationId { get; set; }
  public string? Priority { get; set; }
  public Guid CreatedByUserId { get; set; }
  public DateTimeOffset OccurredAtUtc { get; set; }
}