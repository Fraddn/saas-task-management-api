namespace ProjectSaas.Worker.Configuration;

public sealed class RealtimeDeliveryOptions
{
  public const string SectionName = "RealtimeDelivery";

  public string ApiBaseUrl { get; set; } = string.Empty;
  public string InternalApiKey { get; set; } = string.Empty;
}