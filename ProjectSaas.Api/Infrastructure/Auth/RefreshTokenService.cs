using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectSaas.Api.Application.Abstractions.Auth;
using ProjectSaas.Api.Application.Auth;
using ProjectSaas.Api.Domain.Entities;
using ProjectSaas.Api.Infrastructure.Data;
using System.Security.Cryptography;
using ProjectSaas.Api.Application.Abstractions.Security;
using ProjectSaas.Api.Application.Security;

namespace ProjectSaas.Api.Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly IRefreshTokenHasher _hasher;
    private readonly RefreshTokenOptions _options;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly ISecurityEventService _securityEventService;

    public RefreshTokenService(
        AppDbContext db,
        IRefreshTokenHasher hasher,
        IOptions<RefreshTokenOptions> options,
        ISecurityEventService securityEventService,
        ILogger<RefreshTokenService> logger)
    {
        _db = db;
        _hasher = hasher;
        _options = options.Value;
        _securityEventService = securityEventService;
        _logger = logger;
    }

    public async Task<string> IssueAsync(Guid organisationId, Guid userId, Guid familyId, CancellationToken ct)
    {
        var rawToken = GenerateToken();
        var tokenHash = _hasher.Hash(rawToken);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            OrganisationId = organisationId,
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_options.DaysToLive),
            RevokedAtUtc = null,
            ReplacedByTokenHash = null
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        return rawToken;
    }

    public async Task<RefreshTokenValidationResult?> RotateAsync(
        string rawRefreshToken,
        string? requestIpAddress,
        CancellationToken ct)
    {
        var tokenHash = _hasher.Hash(rawRefreshToken);

        var existing = await _db.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (existing is null)
            return null;

        if (existing.RevokedAtUtc is not null)
        {
            var occurredAtUtc = DateTimeOffset.UtcNow;

            _logger.LogWarning(
                "Refresh token reuse detected. UserId: {UserId}, OrganisationId: {OrganisationId}, FamilyId: {FamilyId}, OccurredAtUtc: {OccurredAtUtc}, RequestIpAddress: {RequestIpAddress}",
                existing.UserId,
                existing.OrganisationId,
                existing.FamilyId,
                occurredAtUtc,
                requestIpAddress);

            await _securityEventService.WriteAsync(
                eventType: SecurityEventTypes.RefreshTokenReuseDetected,
                userId: existing.UserId,
                organisationId: existing.OrganisationId,
                familyId: existing.FamilyId,
                requestIpAddress: requestIpAddress,
                occurredAtUtc: occurredAtUtc,
                metadataJson: null,
                ct: ct);

            await RevokeFamilyAsync(existing.FamilyId, ct);
            await _db.SaveChangesAsync(ct);
            return null;
        }

        if (existing.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            return null;

        var newRawToken = GenerateToken();
        var newTokenHash = _hasher.Hash(newRawToken);

        existing.RevokedAtUtc = DateTimeOffset.UtcNow;
        existing.ReplacedByTokenHash = newTokenHash;

        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            FamilyId = existing.FamilyId,
            OrganisationId = existing.OrganisationId,
            UserId = existing.UserId,
            TokenHash = newTokenHash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_options.DaysToLive),
            RevokedAtUtc = null,
            ReplacedByTokenHash = null
        };

        _db.RefreshTokens.Add(replacement);
        await _db.SaveChangesAsync(ct);

        return new RefreshTokenValidationResult(
            newRawToken,
            existing.UserId,
            existing.OrganisationId
        );
    }

    public async Task RevokeAsync(string rawRefreshToken, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var tokenHash = _hasher.Hash(rawRefreshToken);

        var existing = await _db.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (existing is null)
            return;

        if (existing.RevokedAtUtc is not null)
            return;

        if (existing.ExpiresAtUtc <= now)
            return;

        existing.RevokedAtUtc = now;

        await _db.SaveChangesAsync(ct);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private async Task RevokeFamilyAsync(Guid familyId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var familyTokens = await _db.RefreshTokens
            .Where(x => x.FamilyId == familyId && x.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var token in familyTokens)
        {
            token.RevokedAtUtc = now;
        }
    }
}