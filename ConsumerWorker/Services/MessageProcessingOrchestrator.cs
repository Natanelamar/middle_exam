namespace ConsumerWorker.Services;

public class MessageProcessingOrchestrator
{
    private readonly KafkaConsumerService _kafkaConsumer;
    private readonly DataProcessingService _dataProcessingService;

    public MessageProcessingOrchestrator(
        KafkaConsumerService kafkaConsumer,
        DataProcessingService dataProcessingService)
    {
        _kafkaConsumer = kafkaConsumer;
        _dataProcessingService = dataProcessingService;
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("🚀 Message processing orchestrator started");

        while (!cancellationToken.IsCancellationRequested)
        {
            var message = _kafkaConsumer.ConsumeMessage(cancellationToken);
            
            if (message == null)
                continue;

            Console.WriteLine("📨 Received message");

            await _dataProcessingService.ProcessFieldReportAsync(message);
        }

        Console.WriteLine("🛑 Message processing orchestrator stopped");
    }
}
