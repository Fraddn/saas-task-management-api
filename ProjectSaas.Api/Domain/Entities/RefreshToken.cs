using System;

namespace ProjectSaas.Api.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Guid OrganisationId { get; set; }
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public bool IsRevoked => RevokedAtUtc != null;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;
}