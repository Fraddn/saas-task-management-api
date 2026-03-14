namespace ProjectSaas.Api.Application.Auth;

public sealed record LoginResult(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Email,
    string Role,
    Guid OrganisationId
);