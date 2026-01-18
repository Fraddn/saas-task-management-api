namespace ProjectSaas.Api.Application.Auth;

public sealed record LoginResult(
    string AccessToken,
    Guid UserId,
    string Email,
    string Role,
    Guid OrganisationId
);
