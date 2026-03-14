using ProjectSaas.Api.Application.Auth;
using ProjectSaas.Api.Contracts.Requests.Auth;

namespace ProjectSaas.Api.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct);

    Task<LoginResult> RefreshAsync(
        string refreshToken,
        string? requestIpAddress,
        CancellationToken ct);

    Task LogoutAsync(string refreshToken, CancellationToken ct);
}