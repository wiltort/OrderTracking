using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderTracking.Domain.Interfaces;

namespace OrderTracking.Infrastructure.Messaging.Kafka
{
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string DefaultTopic { get; set; } = "order-events";
    }

    public class KafkaEventPublisher : IEventPublisher
    {
        private readonly IProducer<string, string> _producer;
        private readonly KafkaConfig _config;
        private readonly ILogger<KafkaEventPublisher> _logger;

        public KafkaEventPublisher(
            IOptions<KafkaConfig> options,
            ILogger<KafkaEventPublisher> logger)
        {
            _config = options.Value ?? throw new InvalidOperationException("Kafka configuration is not configured");
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                ClientId = "order-tracking-service",
                Acks = Acks.All,
                MessageTimeoutMs = 5000,
                EnableDeliveryReports = true,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) => 
                    _logger.LogError("Kafka error: {Error}", e.Reason))
                .Build();
        }

        public async Task PublishAsync<T>(T @event, string? topic = null)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            try
            {
                var topicName = topic ?? _config.DefaultTopic;
                var key = @event.GetType().Name;
                var value = JsonSerializer.Serialize(@event);

                var message = new Message<string, string>
                {
                    Key = key,
                    Value = value,
                    Headers = new Headers
                    {
                        { "event-type", System.Text.Encoding.UTF8.GetBytes(key) },
                        { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o")) }
                    }
                };

                var result = await _producer.ProduceAsync(topicName, message);
                
                _logger.LogInformation("Event published to Kafka: Topic={Topic}, Partition={Partition}, Offset={Offset}, Key={Key}", 
                    topicName, result.Partition, result.Offset, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event to Kafka: {EventType}", typeof(T).Name);
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}