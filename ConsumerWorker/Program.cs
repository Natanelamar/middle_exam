using System.Text.Json;
using Confluent.Kafka;
using ConsumerWorker.Data;
using ConsumerWorker.Dto;
using ConsumerWorker.Enums;
using ConsumerWorker.Models;
using ConsumerWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== IronGrid Consumer Worker ===\n");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddSingleton<ConfigurationService>();

var configService = services.BuildServiceProvider().GetRequiredService<ConfigurationService>();

var optionsBuilder = new DbContextOptionsBuilder<IronGridDbContext>();
optionsBuilder.UseMySql(configService.ConnectionString, ServerVersion.AutoDetect(configService.ConnectionString));

using (var dbContext = new IronGridDbContext(optionsBuilder.Options))
{
    Console.WriteLine("Creating database tables...");
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database ready");
}

services.AddDbContext<IronGridDbContext>
(option => option.UseMySql(configService.ConnectionString, ServerVersion.AutoDetect(configService.ConnectionString)));


services.AddSingleton<ValidationService>();
services.AddSingleton<DataProcessingService>();
services.AddSingleton<KafkaConsumerService>();
services.AddSingleton<MessageProcessingOrchestrator>();

var serviceProvider = services.BuildServiceProvider();

var orchestrator = serviceProvider.GetRequiredService<MessageProcessingOrchestrator>();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("\nShutdown requested...");
    cts.Cancel();
    e.Cancel = true;
};

try
{
    await orchestrator.StartProcessingAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Consumer stopped");
}


