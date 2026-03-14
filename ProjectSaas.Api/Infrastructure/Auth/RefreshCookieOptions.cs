namespace ProjectSaas.Api.Infrastructure.Auth;

public sealed class RefreshCookieOptions
{
  public const string SectionName = "Auth:RefreshCookie";

  public string Name { get; set; } = "refresh_token";
  public bool HttpOnly { get; set; } = true;
  public bool Secure { get; set; }
  public string SameSite { get; set; } = "Lax";
  public string Path { get; set; } = "/api/auth";
}