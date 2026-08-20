using ConsumerWorker.Data;
using ConsumerWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("🚀 IronGrid Consumer Worker Starting...");

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Setup DI container
var services = new ServiceCollection();

// Register DbContext
var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<IronGridDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register services
services.AddScoped<ValidationService>();
services.AddScoped<DataProcessingService>();

// Register Kafka consumer as singleton
var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var kafkaGroupId = configuration["Kafka:GroupId"] ?? "irongrid-consumer-group";
var kafkaTopic = configuration["Kafka:Topic"] ?? "field-reports";

services.AddSingleton(new KafkaConsumerService(kafkaBootstrapServers, kafkaGroupId, kafkaTopic));

// Register orchestrator
services.AddScoped<MessageProcessingOrchestrator>();

var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("✓ Services configured");
Console.WriteLine("✓ Database connection ready");
Console.WriteLine($"✓ Kafka configured: {kafkaBootstrapServers}");

// Start the orchestrator
using var scope = serviceProvider.CreateScope();
var orchestrator = scope.ServiceProvider.GetRequiredService<MessageProcessingOrchestrator>();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("\n🛑 Shutdown requested...");
    cts.Cancel();
    e.Cancel = true;
};

await orchestrator.StartProcessingAsync(cts.Token);
