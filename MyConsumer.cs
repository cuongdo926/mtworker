using MassTransit;
using MassTransit.AzureServiceBusTransport;

namespace MasstransitWorker;

public record MyMessage
{
    public string Content { get; set; }
}

internal class MyConsumer(ILogger<MyConsumer> logger) : IConsumer<MyMessage>
{
    public async Task Consume(ConsumeContext<MyMessage> context)
    {
        var bc = context.ReceiveContext as ServiceBusReceiveContext;
        logger.LogInformation($"DeliveryCount: {bc?.DeliveryCount}. Processing...");
        await Task.Delay(TimeSpan.FromSeconds(10), context.CancellationToken);
        logger.LogInformation("Done");
    }
}