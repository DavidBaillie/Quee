﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Quee.Extensions;
using Quee.Tests.Queues.Commands;
using Quee.Tests.Queues.Consumers;

namespace Quee.Tests.Integration.Setup;

internal class QueeWebApplicationFactory : WebApplicationFactory<WebApp.Program>
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
                    .AddSenderAndConsumer<LongRunningTaskCommand, LongRunningTaskConsumer>(nameof(LongRunningTaskCommand));
            });
        });
    }
}
