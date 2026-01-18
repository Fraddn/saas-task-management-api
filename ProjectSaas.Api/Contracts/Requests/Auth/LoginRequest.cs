using System.ComponentModel.DataAnnotations;

namespace ProjectSaas.Api.Contracts.Requests.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;

    [Required]
    public string CompanySlug { get; init; } = null!;
}
