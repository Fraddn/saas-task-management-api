namespace ProjectSaas.Api.Domain.Entities;

public sealed class Notification
{
  public Guid Id { get; set; }

  public Guid OrganisationId { get; set; }

  public Guid UserId { get; set; }

  public string Type { get; set; } = default!;

  public string Title { get; set; } = default!;

  public string Message { get; set; } = default!;

  public bool IsRead { get; set; }

  public DateTimeOffset CreatedAtUtc { get; set; }

  public DateTimeOffset? ReadAtUtc { get; set; }

  public Guid? RelatedEntityId { get; set; }

  public string? RelatedEntityType { get; set; }
}