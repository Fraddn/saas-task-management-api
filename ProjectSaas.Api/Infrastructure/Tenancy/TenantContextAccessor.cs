using ProjectSaas.Api.Application.Abstractions.Tenancy;

namespace ProjectSaas.Api.Infrastructure.Tenancy;

public sealed class TenantContextAccessor : ITenantContext
{
    public Guid OrganisationId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public bool IsAuthenticated { get; private set; }

    public void Set(Guid organisationId, Guid userId, string role)
    {
        OrganisationId = organisationId;
        UserId = userId;
        Role = role;
        IsAuthenticated = true;
    }
}
