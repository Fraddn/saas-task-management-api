namespace ProjectSaas.Api.Configuration;

public sealed class InternalCallbackOptions
{
  public const string SectionName = "InternalCallback";

  public string ApiKey { get; set; } = string.Empty;
}