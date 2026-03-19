using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketCompletedHandler : IOutboxEventHandler
{
  private readonly ILogger<TicketCompletedHandler> _logger;

  public TicketCompletedHandler(ILogger<TicketCompletedHandler> logger)
  {
    _logger = logger;
  }

  public string EventType => "TicketCompleted";

  public Task HandleAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
  {
    var payload = JsonSerializer.Deserialize<TicketCompletedEvent>(
        message.PayloadJson,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (payload is null)
    {
      throw new InvalidOperationException(
          $"Failed to deserialize TicketCompleted payload for outbox message {message.Id}.");
    }

    _logger.LogInformation(
        "Handled {EventType} for TicketId {TicketId} in OrganisationId {OrganisationId}. CompletedByUserId {CompletedByUserId}.",
        message.EventType,
        payload.TicketId,
        payload.OrganisationId,
        payload.CompletedByUserId);

    return Task.CompletedTask;
  }
}

internal sealed class TicketCompletedEvent
{
  public Guid TicketId { get; set; }
  public Guid OrganisationId { get; set; }
  public Guid CompletedByUserId { get; set; }
  public DateTimeOffset OccurredAtUtc { get; set; }
}