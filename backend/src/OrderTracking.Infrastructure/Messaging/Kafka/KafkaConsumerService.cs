using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderTracking.Application.Interfaces;

namespace OrderTracking.Infrastructure.Messaging.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaConfig _config;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaConsumerService(
        IOptions<KafkaConfig> options,
        ILogger<KafkaConsumerService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _config = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = "order-tracking-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true,
            EnablePartitionEof = true
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, e) =>
                _logger.LogError("Kafka consumer error: {Error}", e.Reason))
            .SetStatisticsHandler((_, stats) =>
                _logger.LogDebug("Kafka stats: {Stats}", stats))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka consumer service started");

        // Ensure the topic exists before subscribing
        await EnsureTopicExistsAsync(stoppingToken);

        _consumer.Subscribe(_config.DefaultTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message == null)
                        continue;

                    _logger.LogInformation(
                        "Received message: Key={Key}, Value={Value}, Partition={Partition}, Offset={Offset}",
                        consumeResult.Message.Key,
                        consumeResult.Message.Value,
                        consumeResult.Partition,
                        consumeResult.Offset);

                    await ProcessMessageAsync(consumeResult.Message, stoppingToken);

                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in consumer loop");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopped");
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer closed");
        }
    }

    private async Task ProcessMessageAsync(Message<string, string> message, CancellationToken cancellationToken)
    {
        try
        {
            var eventType = message.Key;
            var eventData = message.Value;

            switch (eventType)
            {
                case nameof(OrderStatusChangedEvent):
                    var orderEvent = JsonSerializer.Deserialize<OrderStatusChangedEvent>(eventData);
                    if (orderEvent != null)
                    {
                        await HandleOrderStatusChangedAsync(orderEvent, cancellationToken);
                    }
                    break;

                case nameof(OrderCreatedEvent):
                    var createdEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(eventData);
                    if (createdEvent != null)
                    {
                        await HandleOrderCreatedAsync(createdEvent, cancellationToken);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown event type: {EventType}", eventType);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing message: {Value}", message.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Key}", message.Key);
        }
    }

    private async Task HandleOrderStatusChangedAsync(OrderStatusChangedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing order status change: Order {OrderNumber} status changed from {OldStatus} to {NewStatus}",
            @event.OrderNumber,
            @event.OldStatus,
            @event.NewStatus);

        using (var scope = _scopeFactory.CreateScope())
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.NotifyOrderStatusChangedWithOldStatusAsync(
                @event.OrderId,
                @event.OrderNumber,
                @event.OldStatus,
                @event.NewStatus,
                @event.UpdatedAt,
                cancellationToken);
        }

        _logger.LogInformation("WebSocket notification sent for order {OrderNumber}", @event.OrderNumber);
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing new order: Order {OrderNumber} created at {CreatedAt}",
            @event.OrderNumber,
            @event.CreatedAt);

        using (var scope = _scopeFactory.CreateScope())
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notificationService.NotifyNewOrderCreatedAsync(
                @event.OrderId,
                @event.OrderNumber,
                @event.Description,
                @event.Status,
                @event.CreatedAt,
                cancellationToken);
        }

        _logger.LogInformation("New order notification sent for {OrderNumber}", @event.OrderNumber);
    }

    /// <summary>
    /// Ensures the configured Kafka topic exists. If it doesn't, creates it.
    /// Retries with backoff until Kafka is available or cancellation is requested.
    /// </summary>
    private async Task EnsureTopicExistsAsync(CancellationToken cancellationToken)
    {
        var topicName = _config.DefaultTopic;
        var maxRetries = 10;
        var retryDelayMs = 3000;

        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _config.BootstrapServers,
            ClientId = "order-tracking-admin"
        }).Build();

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(5));

                var topicExists = metadata.Topics
                    .Any(t => t.Topic == topicName && t.Error.Code == ErrorCode.NoError);

                if (topicExists)
                {
                    _logger.LogInformation("Kafka topic '{Topic}' already exists", topicName);
                    return;
                }

                // Topic doesn't exist — create it
                _logger.LogInformation(
                    "Kafka topic '{Topic}' not found, creating it (attempt {Attempt}/{MaxRetries})",
                    topicName, attempt, maxRetries);

                await adminClient.CreateTopicsAsync(
                [
                    new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    }
                ], new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(10) });

                _logger.LogInformation("Kafka topic '{Topic}' created successfully", topicName);
                return;
            }
            catch (CreateTopicsException ex) when (
                ex.Error.Code == ErrorCode.TopicAlreadyExists ||
                ex.Results?.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists) == true)
            {
                _logger.LogInformation("Kafka topic '{Topic}' already exists (race condition)", topicName);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to verify/create Kafka topic '{Topic}' (attempt {Attempt}/{MaxRetries}), retrying in {Delay}ms",
                    topicName, attempt, maxRetries, retryDelayMs);

                if (attempt == maxRetries)
                {
                    _logger.LogError(
                        ex,
                        "Could not create Kafka topic '{Topic}' after {MaxRetries} attempts. Consumer will attempt to subscribe anyway.",
                        topicName, maxRetries);
                    return;
                }

                await Task.Delay(retryDelayMs, cancellationToken);
            }
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

public class OrderStatusChangedEvent
{
    public Guid OrderId { get; set; }
    public required string OrderNumber { get; set; }
    public required string OldStatus { get; set; }
    public required string NewStatus { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public required string OrderNumber { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}