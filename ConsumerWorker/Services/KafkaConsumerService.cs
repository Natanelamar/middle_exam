using Confluent.Kafka;
using ConsumerWorker.Services;

namespace ConsumerWorker.Services;

public class KafkaConsumerService
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly List<string> _topics;

    public KafkaConsumerService(ConfigurationService config)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config.KafkaBootstrapServers,
            GroupId = config.KafkaGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        _topics = config.KafkaTopics;
        _consumer.Subscribe(_topics);
        
        Console.WriteLine($"✓ Kafka Consumer subscribed to: {string.Join(", ", _topics)}");
    }

    public string? ConsumeMessage(CancellationToken cancellationToken)
    {
        try
        {
            var result = _consumer.Consume(TimeSpan.FromSeconds(1));
            return result?.Message?.Value;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Consume error: {ex.Message}");
            return null;
        }
    }
}
