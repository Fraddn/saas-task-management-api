using ProjectSaas.Api.Application.Auth;
using ProjectSaas.Api.Contracts.Requests.Auth;

namespace ProjectSaas.Api.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct);
}
