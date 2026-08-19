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
        var unitsTopic = configuration["Kafka:Topic:Units"];
        var assetsTopic = configuration["Kafka:Topic:Assets"];
        var assetsLiveStatusTopic = configuration["Kafka:Topic:AssetsLiveStatus"];

        var dataloader = new DataLoader();
        var units = dataloader.LoadData<Unit>(configuration["DataFiles:UnitsPath"]!);
        var assets = dataloader.LoadData<Asset>(configuration["DataFiles:AssetsPath"]!);
        var fieldReports = dataloader.LoadData<AssetLiveStatus>(configuration["DataFiles:FieldReportsPath"]!);

        var kafkaProducer = new KafkaProducerService(kafkaBootstrapServers!);

        var maxCount = Math.Max(units.Count, Math.Max(assets.Count, fieldReports.Count));

        for (int i = 0; i < maxCount; i++)
        {
            if (i < units.Count)
            {
                await kafkaProducer.SendMessageAsync(unitsTopic!, units[i]);
            }
            if (i < assets.Count)
            {
                await kafkaProducer.SendMessageAsync(assetsTopic!, assets[i]);
            }
            if (i < fieldReports.Count)
            {
                await kafkaProducer.SendMessageAsync(assetsLiveStatusTopic!, fieldReports[i]);
            }
            
            await Task.Delay(3000);
        }
        Console.WriteLine("All messages sent!");

        
    }
}