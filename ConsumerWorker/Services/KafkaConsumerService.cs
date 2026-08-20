using Confluent.Kafka;

namespace ConsumerWorker.Services;

public class KafkaConsumerService
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly List<string> _topics;

    public KafkaConsumerService(string bootstrapServers, string groupId, List<string> topics)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();
        _topics = topics;
        _consumer.Subscribe(_topics);
        
        Console.WriteLine($"Kafka Consumer initialized for topics: {string.Join(", ", _topics)}");
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
