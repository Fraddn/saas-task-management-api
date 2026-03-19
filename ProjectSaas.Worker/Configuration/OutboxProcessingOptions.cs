namespace ProjectSaas.Worker.Configuration;

public sealed class OutboxProcessingOptions
{
  public const string SectionName = "OutboxProcessing";

  public int BatchSize { get; set; } = 50;
  public int PollingIntervalSeconds { get; set; } = 3;
  public int MaxRetryCount { get; set; } = 3;
}