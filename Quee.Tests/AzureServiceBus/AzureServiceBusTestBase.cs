using Microsoft.Extensions.DependencyInjection;
using Quee.Tests.Integration.Setup;

namespace Quee.Tests.Integration;

/// <summary>
/// Base class for all integration tests to centralize logic. Enables parallelizable processing by default. 
/// </summary>
[Parallelizable(ParallelScope.All), TestFixture, Category("AzureServiceBus")]
internal abstract class AzureServiceBusTestBase
{
    /// <summary>
    /// Web Application factory to simulate API
    /// </summary>
    protected readonly AzureServiceBusWebApplicationFactory WebApplicationFactory = new();

    /// <summary>
    /// Creates a disposable <see cref="HttpClient"/> to make calls to the API with
    /// </summary>
    /// <returns><see cref="HttpClient"/> configured for contacting Web Application</returns>
    protected HttpClient CreateHttpClient() => WebApplicationFactory.CreateClient();

    /// <summary>
    /// Creates a disposable <see cref="IServiceScope"/> to be used when accessing contents of the DI container
    /// </summary>
    /// <returns><see cref="IServiceScope"/> with access to the Web Application DI container</returns>
    protected IServiceScope CreateScope() => WebApplicationFactory.Services.CreateScope();
}
