using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Quee.Extensions;
using Quee.Tests.Queues.Commands;
using Quee.Tests.Queues.Consumers;

namespace Quee.Tests.Integration.Setup;

/// <summary>
/// Sets up a <see cref="WebApplicationFactory{TEntryPoint}"/> using the Azure Service Bus provider to test how the queue 
/// functions when using Azure Service Bus
/// </summary>
internal class AzureServiceBusWebApplicationFactory : WebApplicationFactory<WebApp.Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Load the test settings into memory to use them
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("./testsettings.json", false)
            .Build();

        // Customize DI container
        builder.ConfigureServices(services =>
        {
            // Setup some queues to process
            services.QueeWithAzureServiceBus(configuration["AzureServiceBusConnectionString"]!, options =>
            {
                options.DisableRetryPolicy()
                    .AddMessageTracker()
                    .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>("quee-test-lrt")
                    .AddSenderAndConsumer<SimpleMessageCommand, SimpleMessageConsumer>("quee-test-sm");
            });
        });
    }
}
