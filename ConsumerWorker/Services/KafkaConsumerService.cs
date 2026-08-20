using Confluent.Kafka;

namespace ConsumerWorker.Services;

public class KafkaConsumerService
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic;

    public KafkaConsumerService(string bootstrapServers, string groupId, string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();
        _topic = topic;
        _consumer.Subscribe(_topic);
        
        Console.WriteLine($"📡 Kafka Consumer initialized for topic: {_topic}");
    }

    public string? ConsumeMessage(CancellationToken cancellationToken)
    {
        try
        {
            var result = _consumer.Consume(cancellationToken);
            return result?.Message?.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Consume error: {ex.Message}");
            return null;
        }
    }
}
