namespace ProjectSaas.Api.Contracts.Responses.Users;

public sealed class UserDto
{
  public Guid Id { get; init; }
  public string Email { get; init; } = string.Empty;
  public string FirstName { get; set; } = default!;
  public string LastName { get; set; } = default!;
  public string Role { get; init; } = string.Empty;
  public bool IsDisabled { get; init; }
  public Guid OrganisationId { get; init; }
  public DateTimeOffset CreatedAtUtc { get; init; }
  public DateTimeOffset UpdatedAtUtc { get; init; }
}