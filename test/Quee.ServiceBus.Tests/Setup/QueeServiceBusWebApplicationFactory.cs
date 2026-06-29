using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Quee.WebApp.Queues.Commands;
using Quee.WebApp.Queues.Consumers;

namespace Quee.ServiceBus.Tests.Setup;

internal class QueeServiceBusWebApplicationFactory : WebApplicationFactory<WebApp.Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Setup some queues to process
            services.QueeWithAzureServiceBus(
                connectionString: "Endpoint=sb://localhost:5300;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
                allowQueueManagement: false,
                configuration: options =>
                {
                    options.DisableRetryPolicy()
                        .AddMessageTracker()
                        .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>(nameof(LongRunningTaskCommand))
                        .AddSenderAndConsumer<SimpleMessageCommand, SimpleMessageConsumer>(nameof(SimpleMessageCommand));
                });
        });
    }
}
