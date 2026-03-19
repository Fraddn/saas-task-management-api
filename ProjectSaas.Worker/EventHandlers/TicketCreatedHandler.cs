using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketCreatedHandler : IOutboxEventHandler
{
  private readonly ILogger<TicketCreatedHandler> _logger;

  public TicketCreatedHandler(ILogger<TicketCreatedHandler> logger)
  {
    _logger = logger;
  }

  public string EventType => "TicketCreated";

  public Task HandleAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
  {
    var payload = JsonSerializer.Deserialize<TicketCreatedEvent>(
        message.PayloadJson,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (payload is null)
    {
      throw new InvalidOperationException(
          $"Failed to deserialize TicketCreated payload for outbox message {message.Id}.");
    }

    _logger.LogInformation(
        "Handled {EventType} for TicketId {TicketId} in OrganisationId {OrganisationId}.",
        message.EventType,
        payload.TicketId,
        payload.OrganisationId);

    return Task.CompletedTask;
  }
}

internal sealed class TicketCreatedEvent
{
  public Guid TicketId { get; set; }
  public Guid OrganisationId { get; set; }
  public string? Priority { get; set; }
  public Guid CreatedByUserId { get; set; }
  public DateTimeOffset OccurredAtUtc { get; set; }
}