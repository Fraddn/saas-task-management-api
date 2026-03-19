using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketAssignedHandler : IOutboxEventHandler
{
  private readonly ILogger<TicketAssignedHandler> _logger;

  public TicketAssignedHandler(ILogger<TicketAssignedHandler> logger)
  {
    _logger = logger;
  }

  public string EventType => "TicketAssigned";

  public Task HandleAsync(OutboxMessageRecord message, CancellationToken cancellationToken)
  {
    var payload = JsonSerializer.Deserialize<TicketAssignedEvent>(
        message.PayloadJson,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (payload is null)
    {
      throw new InvalidOperationException(
          $"Failed to deserialize TicketAssigned payload for outbox message {message.Id}.");
    }

    _logger.LogInformation(
        "Handled {EventType} for TicketId {TicketId} assigned to UserId {AssignedToUserId} in OrganisationId {OrganisationId}.",
        message.EventType,
        payload.TicketId,
        payload.AssignedToUserId,
        payload.OrganisationId);

    return Task.CompletedTask;
  }
}

internal sealed class TicketAssignedEvent
{
  public Guid TicketId { get; set; }
  public Guid OrganisationId { get; set; }
  public Guid AssignedToUserId { get; set; }
  public Guid AssignedByUserId { get; set; }
  public DateTimeOffset OccurredAtUtc { get; set; }
}