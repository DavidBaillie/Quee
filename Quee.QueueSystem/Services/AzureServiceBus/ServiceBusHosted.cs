using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Quee.Interfaces;
using System.Text;

namespace Quee.Services.AzureServiceBus;

public class AzureServiceBusHostedService<TMessage, TConsumer>
    : IHostedService, IDisposable
    where TConsumer : IConsumer<TMessage>
    where TMessage : class
{
    private readonly string connectionString;
    private readonly string queueName;
    private readonly ServiceBusClient serviceBusClient;
    private readonly ServiceBusProcessor serviceBusProcessor;
    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    /// Build a single connection to the Service Bus to handle processing all messages for the provided queue
    /// </summary>
    /// <param name="connectionString">Connection string for the Azure Service Bus</param>
    /// <param name="queueName">Queue to work with in the service bus</param>
    public AzureServiceBusHostedService(string connectionString, string queueName)
    {
        // Save data
        this.connectionString = connectionString;
        this.queueName = queueName;

        // Build the client for connecting to the service bus
        serviceBusClient = new ServiceBusClient(connectionString);
        serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
        serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
        await serviceBusProcessor.StartProcessingAsync(cancellationTokenSource.Token);
    }

    /// <summary>
    /// Processes a single message from the queue
    /// </summary>
    /// <param name="args">Message arguments</param>
    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Message.Body.ToArray());
        Console.WriteLine($"Received message: {body}");

        await args.CompleteMessageAsync(args.Message);

    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource?.Cancel();
        await serviceBusProcessor.StopProcessingAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        cancellationTokenSource?.Dispose();
        serviceBusProcessor.DisposeAsync().GetAwaiter().GetResult();
        serviceBusClient.DisposeAsync().GetAwaiter().GetResult();
    }
}
