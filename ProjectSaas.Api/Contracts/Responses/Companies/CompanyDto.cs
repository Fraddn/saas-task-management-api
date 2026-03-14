namespace ProjectSaas.Api.Contracts.Responses.Companies;

public sealed class CompanyDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Slug { get; init; } = string.Empty;
  public bool IsDeleted { get; init; }
  public DateTimeOffset CreatedAtUtc { get; init; }
  public DateTimeOffset UpdatedAtUtc { get; init; }
}