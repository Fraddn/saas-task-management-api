using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjectSaas.Worker.Models;
using ProjectSaas.Worker.Persistence;
using ProjectSaas.Worker.Services.Interfaces;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketCreatedEventHandler : IOutboxEventHandler
{
  public string EventType => "TicketCreated";

  private readonly WorkerDbContext _db;
  private readonly INotificationDispatchBuffer _notificationDispatchBuffer;

  public TicketCreatedEventHandler(
      WorkerDbContext db,
      INotificationDispatchBuffer notificationDispatchBuffer)
  {
    _db = db;
    _notificationDispatchBuffer = notificationDispatchBuffer;
  }

  public async Task HandleAsync(OutboxMessageRecord message, CancellationToken ct)
  {
    var payload = JsonSerializer.Deserialize<TicketCreatedPayload>(message.PayloadJson)
        ?? throw new InvalidOperationException("Invalid TicketCreated payload.");

    var admins = await _db
        .Set<UserLookup>()
        .Where(u => u.OrganisationId == payload.organisationId && u.Role == "Admin")
        .Select(u => u.Id)
        .ToListAsync(ct);

    if (admins.Count == 0)
    {
      return;
    }

    var createdAtUtc = DateTimeOffset.UtcNow;
    var notifications = new List<NotificationRecord>(admins.Count);

    foreach (var adminId in admins)
    {
      var notification = new NotificationRecord
      {
        Id = Guid.NewGuid(),
        OrganisationId = payload.organisationId,
        UserId = adminId,
        Type = "TicketCreated",
        Title = "New ticket created",
        Message = $"A new ticket \"{payload.title}\" was created.",
        CreatedAtUtc = createdAtUtc,
        IsRead = false,
        RelatedEntityId = payload.ticketId,
        RelatedEntityType = "Ticket"
      };

      notifications.Add(notification);
      _notificationDispatchBuffer.Add(notification.Id);
    }

    _db.Notifications.AddRange(notifications);
  }

  private sealed class TicketCreatedPayload
  {
    public Guid ticketId { get; set; }
    public Guid organisationId { get; set; }
    public string title { get; set; } = "";
  }
}