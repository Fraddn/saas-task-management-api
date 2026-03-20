using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Infrastructure.Data;

namespace ProjectSaas.Api.Application.Notifications;

public sealed class NotificationLiveQueryService : INotificationLiveQueryService
{
  private readonly AppDbContext _dbContext;

  public NotificationLiveQueryService(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<NotificationLiveDto?> GetByIdAsync(Guid notificationId, CancellationToken ct)
  {
    return await _dbContext.Notifications
        .AsNoTracking()
        .Where(n => n.Id == notificationId)
        .Select(n => new NotificationLiveDto
        {
          Id = n.Id,
          OrganisationId = n.OrganisationId,
          UserId = n.UserId,
          Type = n.Type,
          Title = n.Title,
          Message = n.Message,
          IsRead = n.IsRead,
          CreatedAtUtc = n.CreatedAtUtc,
          RelatedEntityId = n.RelatedEntityId,
          RelatedEntityType = n.RelatedEntityType
        })
        .FirstOrDefaultAsync(ct);
  }
}