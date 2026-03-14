namespace ProjectSaas.Api.Application.Abstractions.Security;

public interface ITokenService
{
    string CreateAccessToken(TokenUser user);
}

public sealed record TokenUser(
    Guid UserId,
    Guid OrganisationId,
    string Email,
    string Role,
    bool IsPlatformAdmin
);

