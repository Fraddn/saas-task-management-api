namespace ProjectSaas.Api.Application.Notifications;

public sealed class NotificationLiveDto
{
  public Guid Id { get; set; }
  public Guid OrganisationId { get; set; }
  public Guid UserId { get; set; }

  public string Type { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;

  public bool IsRead { get; set; }
  public DateTimeOffset CreatedAtUtc { get; set; }

  public Guid? RelatedEntityId { get; set; }
  public string? RelatedEntityType { get; set; }
}