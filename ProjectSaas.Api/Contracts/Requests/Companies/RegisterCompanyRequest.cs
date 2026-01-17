namespace ProjectSaas.Api.Contracts.Requests.Companies;

public sealed class RegisterCompanyRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanySlug { get; set; } = string.Empty;

    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
