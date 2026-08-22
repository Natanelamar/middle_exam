using Microsoft.Extensions.Configuration;

namespace ProducerService.Services;

public class ConfigurationService
{
    public string KafkaBootstrapServers { get; }
    public List<string> KafkaTopics { get; }
    public string DataFilesPath { get; }

    public ConfigurationService(IConfiguration configuration)
    {
        KafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        KafkaTopics = new List<string>
        {
            configuration["Kafka:Topics:0"] ?? "UAV-Reports",
            configuration["Kafka:Topics:1"] ?? "PerimeterSensor-Reports"
        };

        DataFilesPath = configuration["DataFiles:FieldReportsPath"]
            ?? throw new Exception("Data files path is missing");

        Console.WriteLine("✓ Configuration loaded successfully");
        Console.WriteLine($"  - Kafka Bootstrap Servers: {KafkaBootstrapServers}");
        Console.WriteLine($"  - Topics: {string.Join(", ", KafkaTopics)}");
        Console.WriteLine($"  - Data Files Path: {DataFilesPath}");
    }
}