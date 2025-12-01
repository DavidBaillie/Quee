using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Quee.WebApp.Queues.Commands;
using Quee.WebApp.Queues.Consumers;

namespace Quee.Tests.Integration.Setup;

internal class QueeWebApplicationFactory : WebApplicationFactory<WebApp.Program>
{
    /// <summary>
    /// Because the <see cref="IWebHostBuilder.ConfigureServices(Action{Microsoft.Extensions.DependencyInjection.IServiceCollection})"/> method can called 
    /// multiple times during parallel runs, this boolean controls for having the queues registered exactly once when the first <see cref="QueeWebApplicationFactory"/> is started.
    /// </summary>
    private static bool hasConfiguredQuee = false;

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Customize DI container
        if (!hasConfiguredQuee)
        {
            hasConfiguredQuee = true;

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
}
