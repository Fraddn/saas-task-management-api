using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjectSaas.Worker.Models;
using ProjectSaas.Worker.Persistence;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketCreatedEventHandler : IOutboxEventHandler
{
  public string EventType => "TicketCreated";

  private readonly WorkerDbContext _db;

  public TicketCreatedEventHandler(WorkerDbContext db)
  {
    _db = db;
  }

  public async Task HandleAsync(OutboxMessageRecord message, CancellationToken ct)
  {
    var payload = JsonSerializer.Deserialize<TicketCreatedPayload>(message.PayloadJson)!;

    var admins = await _db
        .Set<UserLookup>()
        .Where(u => u.OrganisationId == payload.organisationId && u.Role == "Admin")
        .Select(u => u.Id)
        .ToListAsync(ct);

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
        CreatedAtUtc = DateTimeOffset.UtcNow,
        IsRead = false,
        RelatedEntityId = payload.ticketId,
        RelatedEntityType = "Ticket"
      };

      _db.Notifications.Add(notification);
    }
  }

  private sealed class TicketCreatedPayload
  {
    public Guid ticketId { get; set; }
    public Guid organisationId { get; set; }
    public string title { get; set; } = "";
  }
}