namespace ProjectSaas.Api.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }

    public Guid OrganisationId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Employee";
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsPlatformAdmin { get; set; }
    public bool IsDisabled { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
