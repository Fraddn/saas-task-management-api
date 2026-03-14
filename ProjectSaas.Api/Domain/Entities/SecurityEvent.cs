using System;

namespace ProjectSaas.Api.Domain.Entities;

public sealed class SecurityEvent
{
  public Guid Id { get; set; }

  public string EventType { get; set; } = null!;

  public Guid? UserId { get; set; }
  public Guid? OrganisationId { get; set; }
  public Guid? FamilyId { get; set; }

  public string? RequestIpAddress { get; set; }

  public DateTimeOffset OccurredAtUtc { get; set; }

  public string? MetadataJson { get; set; }
}