using ProjectSaas.Api.Application.Auth;

namespace ProjectSaas.Api.Application.Abstractions.Auth;

public interface IRefreshTokenService
{
    Task<string> IssueAsync(Guid organisationId, Guid userId, Guid familyId, CancellationToken ct);

    Task<RefreshTokenValidationResult?> RotateAsync(
        string rawRefreshToken,
        string? requestIpAddress,
        CancellationToken ct);

    Task RevokeAsync(
        string rawRefreshToken,
        CancellationToken ct);
}