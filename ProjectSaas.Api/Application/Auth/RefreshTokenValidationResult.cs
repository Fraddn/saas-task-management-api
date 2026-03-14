namespace ProjectSaas.Api.Application.Auth;

public sealed record RefreshTokenValidationResult(
    string RefreshToken,
    Guid UserId,
    Guid OrganisationId
);