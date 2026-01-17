namespace ProjectSaas.Api.Application.Models;

public sealed class RegisterCompanyResult
{
    public Guid OrganisationId { get; init; }
    public Guid AdminUserId { get; init; }
}
