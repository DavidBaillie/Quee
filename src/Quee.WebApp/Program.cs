namespace Quee.WebApp;

public class Program
{
    private const string SimpleQueueName = "quee-simple-message";
    private const string LongRunningQueueName = "quee-long-running-message";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        //builder.Services.QueeWithAzureServiceBus(builder.Configuration["ServiceBusConnectionString"]!, options =>
        //{
        //    options
        //        .AddSenderAndConsumer<SimpleMessageCommand, SimpleMessageConsumer>(SimpleQueueName, TimeSpan.FromSeconds(1))
        //        .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>(LongRunningQueueName, TimeSpan.FromSeconds(1));
            
        //    options
        //        .AddQueueConsumerOptions(SimpleQueueName, new ConsumerOptions()
        //        {
        //            PrefetchLimit = 10,
        //            ConcurrencyLimit = 1,
        //        })
        //        .AddQueueConsumerOptions(LongRunningQueueName, new ConsumerOptions()
        //        {
        //            PrefetchLimit = 10,
        //            ConcurrencyLimit = 1,
        //        });
        //});

        var app = builder.Build();
        app.UseHttpsRedirection();
        //app.MapPost(
        //    "/simple-message", async (
        //    [FromServices] Quee.IQueueSender<SimpleMessageCommand> sender,
        //    [FromServices] ILogger<SimpleMessageCommand> logger,
        //    [FromQuery] int messageCount,
        //    [FromQuery] string message,
        //    CancellationToken cancellationToken) =>
        //{
        //    for (int i = 0; i < messageCount; i++)
        //    {
        //        logger.LogInformation("Sent simple message to quee");
        //        await sender.SendMessageAsync(new SimpleMessageCommand(Guid.NewGuid(), message), cancellationToken);
        //    }
        //});
        //app.MapPost(
        //    "/long-running", async (
        //    [FromServices] Quee.IQueueSender<LongRunningTaskCommand> sender,
        //    [FromServices] ILogger<LongRunningTaskCommand> logger,
        //    [FromQuery] int messageCount,
        //    [FromQuery] int delay,
        //    CancellationToken cancellationToken) =>
        //    {
        //        for (int i = 0; i < messageCount; i++)
        //        {
        //            logger.LogInformation("Sent long task to quee");
        //            await sender.SendMessageAsync(new LongRunningTaskCommand(delay), cancellationToken);
        //        }
        //    });
        app.MapGet("/", () => TypedResults.Ok("Running"));
        app.Run();
    }
}
