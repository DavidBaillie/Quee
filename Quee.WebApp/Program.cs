using Quee.Extensions;
using Quee.WebApp.Queues.Commands;
using Quee.WebApp.Queues.Consumers;

namespace Quee.WebApp;

public class Program
{
    private const string SimpleQueueName = "quee-simple-message";
    private const string LongRunningQueueName = "quee-long-running-message";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.QueeWithAzureServiceBus(builder.Configuration["ServiceBusConnectionString"]!, options =>
        {
            options
                .AddSenderAndConsumer<SimpleMessageCommand, SimpleMessageConsumer>(SimpleQueueName, TimeSpan.FromSeconds(1))
                .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>(LongRunningQueueName, TimeSpan.FromSeconds(1));
            options
                .AddQueueConsumerOptions(SimpleQueueName, new AzureServiceBus.AzureServiceBusConsumerOptions()
                {
                    PrefetchLimit = 10,
                    ConcurrencyLimit = 1
                })
                .AddQueueConsumerOptions(LongRunningQueueName, new AzureServiceBus.AzureServiceBusConsumerOptions()
                {
                    ConcurrencyLimit = 10
                });
        });

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.MapGet("/", () => TypedResults.Ok("Running"));
        app.Run();
    }
}
