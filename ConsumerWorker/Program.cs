using ConsumerWorker.Data;
using ConsumerWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("IronGrid Consumer Worker Starting...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();

var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<IronGridDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

services.AddScoped<ValidationService>();
services.AddScoped<DataProcessingService>();

var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var kafkaGroupId = configuration["Kafka:GroupId"] ?? "irongrid-consumer-group";
var kafkaTopics = new List<string>
{
    configuration["Kafka:Topics:0"] ?? "UAV-Reports",
    configuration["Kafka:Topics:1"] ?? "PerimeterSensor-Reports"
};

services.AddSingleton(new KafkaConsumerService(kafkaBootstrapServers, kafkaGroupId, kafkaTopics));

services.AddScoped<MessageProcessingOrchestrator>();

var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("Services configured");
Console.WriteLine("Database connection ready");
Console.WriteLine($"Kafka configured: {kafkaBootstrapServers}");

using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IronGridDbContext>();
    Console.WriteLine("Ensuring database is created...");
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database and tables created successfully");
}

using var scope2 = serviceProvider.CreateScope();
var orchestrator = scope2.ServiceProvider.GetRequiredService<MessageProcessingOrchestrator>();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("\nShutdown requested...");
    cts.Cancel();
    e.Cancel = true;
};

await orchestrator.StartProcessingAsync(cts.Token);
