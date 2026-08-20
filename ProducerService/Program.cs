using Microsoft.Extensions.Configuration;
using ProducerService.Services;
using ProducerService.Models;

class Program
{

    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"];
        var uavTopic = configuration["Kafka:Topics:UAV"];
        var perimeterSensorTopic = configuration["Kafka:Topics:PerimeterSensor"];

        var dataloader = new DataLoader();
        var fieldReports = dataloader.LoadData<AssetLiveStatus>(configuration["DataFiles:FieldReportsPath"]!);

        var kafkaProducer = new KafkaProducerService(kafkaBootstrapServers!);

        int uavCount = 0;
        int sensorCount = 0;

        foreach (var report in fieldReports)
        {
            string topic = report.AssetType switch
            {
                "UAV" => uavTopic!,
                "PerimeterSensor" => perimeterSensorTopic!,
                _ => throw new Exception($"Unknown asset type: {report.AssetType}")
            };

            await kafkaProducer.SendMessageAsync(topic, report);

            if (report.AssetType == "UAV")
                uavCount++;
            else
                sensorCount++;
        }

        Console.WriteLine("All messages sent!");
        Console.WriteLine($"  - UAV reports: {uavCount}");
        Console.WriteLine($"  - Sensor reports: {sensorCount}");


    }
}