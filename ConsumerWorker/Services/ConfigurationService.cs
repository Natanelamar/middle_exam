using Microsoft.Extensions.Configuration;

namespace ConsumerWorker.Services;

public class ConfigurationService
{
    public string ConnectionString { get; }
    public string KafkaBootstrapServers { get; }
    public string KafkaGroupId { get; }
    public List<string> KafkaTopics { get; }

    public ConfigurationService(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new Exception("Database connection string is missing");

        KafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        KafkaGroupId = configuration["Kafka:GroupId"] ?? "irongrid-consumer-group-v2";

        KafkaTopics = new List<string>
        {
            configuration["Kafka:Topics:0"] ?? "UAV-Reports",
            configuration["Kafka:Topics:1"] ?? "PerimeterSensor-Reports"
        };

        Console.WriteLine("✓ Configuration loaded successfully");
        Console.WriteLine($"  - Kafka Bootstrap Servers: {KafkaBootstrapServers}");
        Console.WriteLine($"  - Kafka Group ID: {KafkaGroupId}");
        Console.WriteLine($"  - Topics: {string.Join(", ", KafkaTopics)}");
    }
}
