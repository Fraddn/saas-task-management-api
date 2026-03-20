using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using ProjectSaas.Worker.Configuration;
using ProjectSaas.Worker.Persistence;
using ProjectSaas.Worker.Services.Interfaces;

namespace ProjectSaas.Worker.Workers;

public sealed class OutboxProcessorWorker : BackgroundService
{
  private readonly ILogger<OutboxProcessorWorker> _logger;
  private readonly OutboxProcessingOptions _options;
  private readonly IServiceScopeFactory _scopeFactory;

  public OutboxProcessorWorker(
      ILogger<OutboxProcessorWorker> logger,
      IOptions<OutboxProcessingOptions> options,
      IServiceScopeFactory scopeFactory)
  {
    _logger = logger;
    _options = options.Value;
    _scopeFactory = scopeFactory;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation(
        "Outbox processor worker started. BatchSize: {BatchSize}, PollingIntervalSeconds: {PollingIntervalSeconds}, MaxRetryCount: {MaxRetryCount}",
        _options.BatchSize,
        _options.PollingIntervalSeconds,
        _options.MaxRetryCount);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        List<Guid> pendingMessageIds;

        using (var scope = _scopeFactory.CreateScope())
        {
          var dbContext = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();

          var batchSizeParameter = new NpgsqlParameter("batchSize", _options.BatchSize);
          var maxRetryParameter = new NpgsqlParameter("maxRetry", _options.MaxRetryCount);

          pendingMessageIds = await dbContext.OutboxMessages
              .FromSqlRaw(@"
                            SELECT *
                            FROM ""OutboxMessages""
                            WHERE ""ProcessedAtUtc"" IS NULL
                              AND ""RetryCount"" < @maxRetry
                            ORDER BY ""OccurredAtUtc""
                            LIMIT @batchSize",
                  maxRetryParameter,
                  batchSizeParameter)
              .Select(x => x.Id)
              .ToListAsync(stoppingToken);
        }

        _logger.LogInformation(
            "Outbox polling cycle fetched {MessageCount} pending messages.",
            pendingMessageIds.Count);

        if (pendingMessageIds.Count == 0)
        {
          await Task.Delay(
              TimeSpan.FromSeconds(_options.PollingIntervalSeconds),
              stoppingToken);

          continue;
        }

        var successCount = 0;
        var failureCount = 0;

        foreach (var messageId in pendingMessageIds)
        {
          try
          {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxEventDispatcher>();
            var realtimeDispatcher = scope.ServiceProvider.GetRequiredService<IRealtimeNotificationDispatcher>();
            var notificationDispatchBuffer = scope.ServiceProvider.GetRequiredService<INotificationDispatchBuffer>();

            notificationDispatchBuffer.Clear();

            await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            var message = await dbContext.OutboxMessages
                .FirstOrDefaultAsync(x => x.Id == messageId, stoppingToken);

            if (message is null)
            {
              _logger.LogWarning(
                  "Outbox message {MessageId} no longer exists. Skipping.",
                  messageId);

              await transaction.RollbackAsync(stoppingToken);
              continue;
            }

            if (message.ProcessedAtUtc is not null)
            {
              _logger.LogInformation(
                  "Outbox message {MessageId} is already processed. Skipping.",
                  messageId);

              await transaction.RollbackAsync(stoppingToken);
              continue;
            }

            await dispatcher.DispatchAsync(message, stoppingToken);

            message.ProcessedAtUtc = DateTime.UtcNow;
            message.LastError = null;

            var notificationIdsToDispatch = notificationDispatchBuffer.GetAll().ToList();

            await dbContext.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);

            foreach (var notificationId in notificationIdsToDispatch)
            {
              try
              {
                await realtimeDispatcher.DispatchAsync(notificationId, stoppingToken);
              }
              catch (Exception liveEx)
              {
                _logger.LogWarning(
                    liveEx,
                    "Notification {NotificationId} was persisted but live delivery failed.",
                    notificationId);
              }
            }

            successCount++;

            _logger.LogInformation(
                "Successfully processed outbox message {MessageId} of type {EventType}.",
                message.Id,
                message.EventType);
          }
          catch (Exception ex)
          {
            failureCount++;

            _logger.LogError(
                ex,
                "Failed processing outbox message {MessageId}. Attempting to record retry state.",
                messageId);

            try
            {
              using var failureScope = _scopeFactory.CreateScope();
              var failureDbContext = failureScope.ServiceProvider.GetRequiredService<WorkerDbContext>();

              var failedMessage = await failureDbContext.OutboxMessages
                  .FirstOrDefaultAsync(x => x.Id == messageId, stoppingToken);

              if (failedMessage is not null && failedMessage.ProcessedAtUtc is null)
              {
                failedMessage.RetryCount += 1;
                failedMessage.LastError = ex.Message;

                await failureDbContext.SaveChangesAsync(stoppingToken);
              }
            }
            catch (Exception retryEx)
            {
              _logger.LogError(
                  retryEx,
                  "Failed recording retry state for outbox message {MessageId}.",
                  messageId);
            }
          }
        }

        _logger.LogInformation(
            "Outbox polling cycle completed. Succeeded: {SuccessCount}, Failed: {FailureCount}.",
            successCount,
            failureCount);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Unhandled error in outbox processor worker loop.");
      }

      await Task.Delay(
          TimeSpan.FromSeconds(_options.PollingIntervalSeconds),
          stoppingToken);
    }

    _logger.LogInformation("Outbox processor worker stopped.");
  }
}