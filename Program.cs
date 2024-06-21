using MassTransit;
using Serilog;

namespace MasstransitWorker;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddSerilog(Log.Logger);
        });

        builder.Services.AddOptions<MassTransitHostOptions>()
            .Configure(x => { x.WaitUntilStarted = true; });

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<MyConsumer>();
            x.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host("");
                cfg.UseRawJsonSerializer(isDefault: true);
                cfg.UseRawJsonDeserializer(isDefault: true);
                cfg.ReceiveEndpoint("events", c =>
                {
                    c.ConfigureConsumer<MyConsumer>(context);
                    c.UseRawJsonSerializer(isDefault: true);
                    c.UseRawJsonDeserializer(isDefault: true);
                    c.ConfigureDeadLetterQueueDeadLetterTransport();
                    c.ConfigureDeadLetterQueueErrorTransport();
                    c.ConfigureConsumeTopology = false;
                    c.MaxConcurrentCalls = 1;
                    c.MaxDeliveryCount = 100;
                    c.DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1);
                    c.RequiresDuplicateDetection = true;
                    c.LockDuration = TimeSpan.FromSeconds(5);
                    c.MaxAutoRenewDuration = TimeSpan.FromSeconds(3);
                    c.UseMessageRetry(a => a.None());

                    c.PublishFaults = false;

                    // I changed the following two configurations, but it didn't help.
                    c.DiscardFaultedMessages();
                    c.RethrowFaultedMessages();
                });
            });
        });

        var host = builder.Build();
        host.Run();
    }
}