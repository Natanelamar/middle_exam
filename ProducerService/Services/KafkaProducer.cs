using System.Text.Json;
using Confluent.Kafka;

namespace ProducerService.Services;

public class KafkaProducerService
{
    private readonly string _bootstrapServers;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(string bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;

        var config = new ProducerConfig { BootstrapServers = _bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendMessageAsync<T>(string topic, T message)
    {
        var json = JsonSerializer.Serialize(message);

        var messageToSend = new Message<Null, string>
        {
            Value = json
        };
        await _producer.ProduceAsync(topic, messageToSend);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sent to {topic}: {json.Substring(0, Math.Min(50, json.Length))}...");
    }
}
