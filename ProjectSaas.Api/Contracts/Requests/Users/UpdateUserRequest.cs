namespace ProjectSaas.Api.Contracts.Requests.Users;

public sealed class UpdateUserRequest
{
  public string? Email { get; init; }
  public string? Role { get; init; }
  public bool? IsDisabled { get; init; }
}