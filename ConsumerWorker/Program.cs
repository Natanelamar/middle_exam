using System.Text.Json;
using Confluent.Kafka;
using ConsumerWorker.Data;
using ConsumerWorker.Dto;
using ConsumerWorker.Enums;
using ConsumerWorker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

Console.WriteLine("IronGrid Consumer Worker Starting...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var kafkaGroupId = configuration["Kafka:GroupId"] ?? "irongrid-consumer-group-v2";
var kafkaTopics = new List<string>
{
    configuration["Kafka:Topics:0"] ?? "UAV-Reports",
    configuration["Kafka:Topics:1"] ?? "PerimeterSensor-Reports"
};

var optionsBuilder = new DbContextOptionsBuilder<IronGridDbContext>();
optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

using (var dbContext = new IronGridDbContext(optionsBuilder.Options))
{
    Console.WriteLine("Creating database tables...");
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database ready");
}

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = kafkaBootstrapServers,
    GroupId = kafkaGroupId,
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
consumer.Subscribe(kafkaTopics);

Console.WriteLine($"Subscribed to: {string.Join(", ", kafkaTopics)}");
Console.WriteLine("Consuming messages...\n");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("\nShutdown requested...");
    cts.Cancel();
    e.Cancel = true;
};

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var result = consumer.Consume(TimeSpan.FromSeconds(1));
        
        if (result == null || result.Message?.Value == null)
            continue;

        Console.WriteLine($"Received message from {result.Topic}");

        using (var dbContext = new IronGridDbContext(optionsBuilder.Options))
        {
            try
            {
                var report = JsonSerializer.Deserialize<FieldReport>(result.Message.Value);
                if (report == null)
                {
                    Console.WriteLine("Failed to deserialize message");
                    continue;
                }

                var asset = await dbContext.Assets.FindAsync(report.AssetId);
                if (asset == null)
                {
                    Console.WriteLine($"Asset {report.AssetId} not found");
                    continue;
                }

                var assetLiveStatus = new AssetLiveStatus
                {
                    AssetId = report.AssetId,
                    AssetType = report.AssetType,
                    RawValue = report.RawValue,
                    LastUpdate = DateTime.UtcNow
                };

                if (report.AssetType == "UAV")
                {
                    assetLiveStatus.IsVerified = ValidateUAV(report.RawValue);
                    assetLiveStatus.ProcessedStatus = CalculateUAVStatus(report.RawValue);
                }
                else if (report.AssetType == "PerimeterSensor")
                {
                    assetLiveStatus.IsVerified = ValidatePerimeterSensor(report.RawValue);
                    assetLiveStatus.ProcessedStatus = CalculatePerimeterSensorStatus(report.RawValue);
                }
                else
                {
                    Console.WriteLine($"Unknown asset type: {report.AssetType}");
                    continue;
                }

                var existingStatus = await dbContext.AssetLiveStatuses
                    .FirstOrDefaultAsync(als => als.AssetId == report.AssetId);

                if (existingStatus != null)
                {
                    existingStatus.AssetType = assetLiveStatus.AssetType;
                    existingStatus.RawValue = assetLiveStatus.RawValue;
                    existingStatus.ProcessedStatus = assetLiveStatus.ProcessedStatus;
                    existingStatus.IsVerified = assetLiveStatus.IsVerified;
                    existingStatus.LastUpdate = assetLiveStatus.LastUpdate;
                }
                else
                {
                    dbContext.AssetLiveStatuses.Add(assetLiveStatus);
                }

                await dbContext.SaveChangesAsync();
                Console.WriteLine($"Processed report for Asset {report.AssetId}: {assetLiveStatus.ProcessedStatus} (Verified: {assetLiveStatus.IsVerified})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Consumer stopped");
}
finally
{
    consumer.Close();
}

bool ValidateUAV(string rawValue)
{
    var normalized = rawValue.Trim().ToLower();
    
    if (int.TryParse(normalized, out int batteryLevel))
    {
        return batteryLevel >= 0 && batteryLevel <= 100;
    }
    
    return normalized == "error" || normalized == "fault" || normalized == "unknown";
}

ProcessedStatus CalculateUAVStatus(string rawValue)
{
    var normalized = rawValue.Trim().ToLower();
    
    if (int.TryParse(normalized, out int batteryLevel))
    {
        if (batteryLevel >= 20)
            return ProcessedStatus.Stable;
        else
            return ProcessedStatus.Warning;
    }
    
    return ProcessedStatus.Warning;
}

bool ValidatePerimeterSensor(string rawValue)
{
    var normalized = rawValue.Trim().ToLower();
    return normalized == "good" || normalized == "gud" || normalized == "bad" || normalized == "bed";
}

ProcessedStatus CalculatePerimeterSensorStatus(string rawValue)
{
    var normalized = rawValue.Trim().ToLower();
    
    if (normalized == "good" || normalized == "gud")
        return ProcessedStatus.Stable;
    
    return ProcessedStatus.Warning;
}
