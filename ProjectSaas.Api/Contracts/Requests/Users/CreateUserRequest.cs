namespace ProjectSaas.Api.Contracts.Requests.Users;

public sealed class CreateUserRequest
{
  public string Email { get; init; } = string.Empty;
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;

  public string Password { get; init; } = string.Empty;
  public string Role { get; init; } = string.Empty;
}