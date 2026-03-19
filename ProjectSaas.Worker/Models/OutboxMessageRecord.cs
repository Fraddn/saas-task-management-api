namespace ProjectSaas.Worker.Models;

public sealed class OutboxMessageRecord
{
  public Guid Id { get; set; }

  public Guid OrganisationId { get; set; }

  public string EventType { get; set; } = string.Empty;

  public string PayloadJson { get; set; } = string.Empty;

  public DateTime OccurredAtUtc { get; set; }

  public DateTime? ProcessedAtUtc { get; set; }

  public int RetryCount { get; set; }

  public string? LastError { get; set; }
}