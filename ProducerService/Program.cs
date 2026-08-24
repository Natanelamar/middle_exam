using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProducerService.Models;
using ProducerService.Services;

namespace ProducerService;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== IronGrid Producer Service ===\n");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<DataLoader>();
        services.AddSingleton<KafkaProducerService>();

        var serviceProvider = services.BuildServiceProvider();

        var configService = serviceProvider.GetRequiredService<ConfigurationService>();
        var dataLoader = serviceProvider.GetRequiredService<DataLoader>();
        var kafkaProducer = serviceProvider.GetRequiredService<KafkaProducerService>();

        var fieldReports = dataLoader.LoadData<AssetLiveStatus>(configService.DataFilesPath);

        Console.WriteLine($"Loaded {fieldReports.Count} field reports\n");

        int uavCount = 0;
        int sensorCount = 0;

        foreach (var report in fieldReports)
        {
            string topic = report.AssetType switch
            {
                "UAV" => configService.KafkaTopics[0],
                "PerimeterSensor" => configService.KafkaTopics[1],
                _ => throw new Exception($"Unknown asset type: {report.AssetType}")
            };

            await kafkaProducer.SendMessageAsync(topic, report);

            if (report.AssetType == "UAV")
                uavCount++;
            else
                sensorCount++;
        }

        Console.WriteLine("\n✓ All messages sent successfully!");
        Console.WriteLine($"  - UAV reports: {uavCount}");
        Console.WriteLine($"  - Sensor reports: {sensorCount}");
    }
}