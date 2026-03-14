using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSaas.Api.Application.Abstractions.Security;

public interface ISecurityEventService
{
  Task WriteAsync(
      string eventType,
      Guid? userId,
      Guid? organisationId,
      Guid? familyId,
      string? requestIpAddress,
      DateTimeOffset occurredAtUtc,
      string? metadataJson,
      CancellationToken ct);
}