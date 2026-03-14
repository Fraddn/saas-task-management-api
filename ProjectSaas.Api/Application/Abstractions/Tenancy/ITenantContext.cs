namespace ProjectSaas.Api.Application.Abstractions.Tenancy;

public interface ITenantContext
{
    Guid OrganisationId { get; }
    Guid UserId { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformAdmin { get; }
}
