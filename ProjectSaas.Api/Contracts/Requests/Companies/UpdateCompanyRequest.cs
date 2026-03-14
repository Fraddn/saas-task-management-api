namespace ProjectSaas.Api.Contracts.Requests.Companies;

public sealed class UpdateCompanyRequest
{
  public string? Name { get; init; }
  public string? Slug { get; init; }
}