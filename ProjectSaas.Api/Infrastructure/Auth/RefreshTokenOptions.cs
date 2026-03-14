namespace ProjectSaas.Api.Infrastructure.Auth;

public sealed class RefreshTokenOptions
{
  public const string SectionName = "RefreshTokens";

  public string Pepper { get; set; } = string.Empty;
  public int DaysToLive { get; set; } = 14;
}