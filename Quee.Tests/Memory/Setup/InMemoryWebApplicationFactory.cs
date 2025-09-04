using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Quee.Extensions;
using Quee.Tests.Queues.Commands;
using Quee.Tests.Queues.Consumers;

namespace Quee.Tests.AzureServiceBus.Setup;

/// <summary>
/// Sets up a <see cref="WebApplicationFactory{TEntryPoint}"/> using the in-memory service provider for testing how the
/// queue handles running when using the in-memory provider.
/// </summary>
internal class InMemoryWebApplicationFactory : WebApplicationFactory<WebApp.Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Customize DI container
        builder.ConfigureServices(services =>
        {
            // Setup some queues to process
            services.QueeInMemory(options =>
            {
                options.DisableRetryPolicy()
                    .AddMessageTracker()
                    .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>(nameof(LongRunningTaskCommand))
                    .AddSenderAndConsumer<SimpleMessageCommand, SimpleMessageConsumer>(nameof(SimpleMessageCommand));
            });
        });
    }
}
