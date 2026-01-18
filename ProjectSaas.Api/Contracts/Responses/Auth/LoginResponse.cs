namespace ProjectSaas.Api.Contracts.Responses.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = null!;
    public AuthUserDto User { get; init; } = null!;
}

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string Role { get; init; } = null!;
    public Guid OrganisationId { get; init; }
}
