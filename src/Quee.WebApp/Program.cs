namespace Quee.WebApp;

public class Program
{
    private const string SimpleQueueName = "quee-simple-message";
    private const string LongRunningQueueName = "quee-long-running-message";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        //builder.Services.QueeWithAzureServiceBus(builder.Configuration["ServiceBusConnectionString"]!, true, options =>
        //{
        //    options
        //        .AddSenderAndConsumer<SimpleMessageCommand, SimpleMessageConsumer>(SimpleQueueName, TimeSpan.FromSeconds(1))
        //        .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>(LongRunningQueueName, TimeSpan.FromSeconds(1));

        //    options
        //        .AddQueueConsumerOptions(SimpleQueueName, new ConsumerOptions()
        //        {
        //            PrefetchLimit = 10,
        //            ConcurrencyLimit = 10,
        //        })
        //        .AddQueueConsumerOptions(LongRunningQueueName, new ConsumerOptions()
        //        {
        //            PrefetchLimit = 10,
        //            ConcurrencyLimit = 10,
        //        });
        //});

        var app = builder.Build();
        app.UseHttpsRedirection();
        //app.MapPost(
        //    "/simple-message", async (
        //    [FromServices] IQueueSender<SimpleMessageCommand> sender,
        //    [FromServices] ILogger<SimpleMessageCommand> logger,
        //    [FromQuery] int messageCount,
        //    [FromQuery] string message,
        //    CancellationToken cancellationToken) =>
        //{
        //    var messageTasks = Enumerable.Range(0, messageCount)
        //        .Select(x => sender.SendMessageAsync(new SimpleMessageCommand(Guid.NewGuid(), message), cancellationToken))
        //        .ToList();

        //    await Task.WhenAll(messageTasks);
        //    return TypedResults.Ok();
        //});
        //app.MapPost(
        //    "/long-running", async (
        //    [FromServices] IQueueSender<LongRunningTaskCommand> sender,
        //    [FromServices] ILogger<LongRunningTaskCommand> logger,
        //    [FromQuery] int messageCount,
        //    [FromQuery] int delay,
        //    CancellationToken cancellationToken) =>
        //    {
        //        var messageTasks = Enumerable.Range(0, messageCount)
        //        .Select(x => sender.SendMessageAsync(new LongRunningTaskCommand(delay), cancellationToken))
        //        .ToList();

        //        await Task.WhenAll(messageTasks);
        //        return TypedResults.Ok();
        //    });
        app.MapGet("/", () => TypedResults.Ok("Running"));
        app.Run();
    }
}
