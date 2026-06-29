namespace Quee.Memory.Tests;

/// <summary>
/// Sets up and tears down all tests under the integration test folder
/// </summary>
[SetUpFixture]
internal class MemoryIntegrationLifeCycle
{
    /// <summary>
    /// Handles setting up the test environment before tests run
    /// </summary>
    [OneTimeSetUp]
    public async Task SetupAsync()
    {

    }

    /// <summary>
    /// Handles cleaning up the test environment after tests complete
    /// </summary>
    [OneTimeTearDown]
    public async Task TearDownAsync()
    {

    }
}
