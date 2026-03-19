using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Application.Abstractions.Tenancy;
using ProjectSaas.Api.Application.Exceptions;
using ProjectSaas.Api.Infrastructure.Data;

namespace ProjectSaas.Api.Application.Notifications;

public sealed class NotificationService : INotificationService
{
  private const int DefaultPage = 1;
  private const int DefaultPageSize = 20;
  private const int MaxPageSize = 100;

  private readonly AppDbContext _db;
  private readonly ITenantContext _tenant;

  public NotificationService(AppDbContext db, ITenantContext tenant)
  {
    _db = db;
    _tenant = tenant;
  }

  public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
      int page,
      int pageSize,
      CancellationToken ct)
  {
    page = page <= 0 ? DefaultPage : page;
    pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
    pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

    var organisationId = _tenant.OrganisationId;
    var userId = _tenant.UserId;

    var notifications = await _db.Notifications
        .AsNoTracking()
        .Where(n => n.OrganisationId == organisationId && n.UserId == userId)
        .OrderByDescending(n => n.CreatedAtUtc)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(n => new NotificationDto
        {
          Id = n.Id,
          Type = n.Type,
          Title = n.Title,
          Message = n.Message,
          IsRead = n.IsRead,
          CreatedAtUtc = n.CreatedAtUtc,
          RelatedEntityId = n.RelatedEntityId,
          RelatedEntityType = n.RelatedEntityType
        })
        .ToListAsync(ct);

    return notifications;
  }

  public async Task MarkAsReadAsync(Guid notificationId, CancellationToken ct)
  {
    var organisationId = _tenant.OrganisationId;
    var userId = _tenant.UserId;

    var notification = await _db.Notifications
        .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

    if (notification is null)
    {
      throw new KeyNotFoundException("Notification not found.");
    }

    if (notification.OrganisationId != organisationId || notification.UserId != userId)
    {
      throw new ForbiddenException("You do not have access to this notification.");
    }

    if (notification.IsRead)
    {
      return;
    }

    notification.IsRead = true;
    notification.ReadAtUtc = DateTimeOffset.UtcNow;

    await _db.SaveChangesAsync(ct);
  }

  public async Task<int> GetUnreadCountAsync(CancellationToken ct)
  {
    var organisationId = _tenant.OrganisationId;
    var userId = _tenant.UserId;

    return await _db.Notifications
        .AsNoTracking()
        .Where(n =>
            n.OrganisationId == organisationId &&
            n.UserId == userId &&
            !n.IsRead)
        .CountAsync(ct);
  }

  public async Task MarkAllAsReadAsync(CancellationToken ct)
  {
    var organisationId = _tenant.OrganisationId;
    var userId = _tenant.UserId;
    var readAtUtc = DateTimeOffset.UtcNow;

    var unreadNotifications = await _db.Notifications
        .Where(n =>
            n.OrganisationId == organisationId &&
            n.UserId == userId &&
            !n.IsRead)
        .ToListAsync(ct);

    if (unreadNotifications.Count == 0)
    {
      return;
    }

    foreach (var notification in unreadNotifications)
    {
      notification.IsRead = true;
      notification.ReadAtUtc = readAtUtc;
    }

    await _db.SaveChangesAsync(ct);
  }
}