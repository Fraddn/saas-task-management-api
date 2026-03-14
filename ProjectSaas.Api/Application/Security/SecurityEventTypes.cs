namespace ProjectSaas.Api.Application.Security;

public static class SecurityEventTypes
{
  public const string RefreshTokenReuseDetected = "refresh_token_reuse_detected";

  public const string LoginRateLimitExceeded = "login_rate_limit_exceeded";

  public const string RefreshRateLimitExceeded = "refresh_rate_limit_exceeded";
}