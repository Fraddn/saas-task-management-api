using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectSaas.Api.Application.Abstractions.Security;
using ProjectSaas.Api.Domain.Entities;
using ProjectSaas.Api.Infrastructure.Data;

namespace ProjectSaas.Api.Infrastructure.Security;

public sealed class SecurityEventService : ISecurityEventService
{
  private readonly AppDbContext _db;

  public SecurityEventService(AppDbContext db)
  {
    _db = db;
  }

  public async Task WriteAsync(
      string eventType,
      Guid? userId,
      Guid? organisationId,
      Guid? familyId,
      string? requestIpAddress,
      DateTimeOffset occurredAtUtc,
      string? metadataJson,
      CancellationToken ct)
  {
    var securityEvent = new SecurityEvent
    {
      Id = Guid.NewGuid(),
      EventType = eventType,
      UserId = userId,
      OrganisationId = organisationId,
      FamilyId = familyId,
      RequestIpAddress = requestIpAddress,
      OccurredAtUtc = occurredAtUtc,
      MetadataJson = metadataJson
    };

    _db.SecurityEvents.Add(securityEvent);
    await _db.SaveChangesAsync(ct);
  }
}