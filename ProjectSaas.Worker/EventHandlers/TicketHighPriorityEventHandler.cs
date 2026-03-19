using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjectSaas.Worker.Models;
using ProjectSaas.Worker.Persistence;

namespace ProjectSaas.Worker.EventHandlers;

public sealed class TicketHighPriorityEventHandler : IOutboxEventHandler
{
  public string EventType => "TicketHighPriority";

  private readonly WorkerDbContext _db;

  public TicketHighPriorityEventHandler(WorkerDbContext db)
  {
    _db = db;
  }

  public async Task HandleAsync(OutboxMessageRecord message, CancellationToken ct)
  {
    var payload = JsonSerializer.Deserialize<Payload>(message.PayloadJson)!;

    var admins = await _db
        .Set<UserLookup>()
        .Where(u => u.OrganisationId == message.OrganisationId && u.Role == "Admin")
        .Select(u => u.Id)
        .ToListAsync(ct);

    foreach (var adminId in admins)
    {
      var notification = new NotificationRecord
      {
        Id = Guid.NewGuid(),
        OrganisationId = message.OrganisationId,
        UserId = adminId,
        Type = "TicketHighPriority",
        Title = "High priority ticket",
        Message = "A ticket has been marked HIGH priority.",
        CreatedAtUtc = DateTimeOffset.UtcNow,
        IsRead = false,
        RelatedEntityId = payload.ticketId,
        RelatedEntityType = "Ticket"
      };

      _db.Notifications.Add(notification);
    }
  }

  private sealed class Payload
  {
    public Guid ticketId { get; set; }
    public string priority { get; set; } = "";
  }
}