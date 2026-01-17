namespace ProjectSaas.Api.Contracts.Responses.Companies;

public sealed class RegisterCompanyResponse
{
    public Guid OrganisationId { get; set; }
    public Guid AdminUserId { get; set; }
}
