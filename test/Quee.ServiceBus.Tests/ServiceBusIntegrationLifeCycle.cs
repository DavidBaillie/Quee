using System.Diagnostics;
using System.Net;

namespace Quee.ServiceBus.Tests;

/// <summary>
/// Sets up and tears down all tests under the integration test folder
/// </summary>
[SetUpFixture]
internal class ServiceBusIntegrationLifeCycle
{
    private static readonly string ComposeFile = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "docker", "docker-compose.yml"));

    /// <summary>
    /// Handles setting up the test environment before tests run
    /// </summary>
    [OneTimeSetUp]
    public async Task SetupAsync()
    {
        TestContext.Progress.WriteLine("Setting up Service Bus Container...");
        await RunDockerComposeAsync("up -d");
        await WaitForEmulatorAsync();
        TestContext.Progress.WriteLine("Container started.");
    }

    /// <summary>
    /// Handles cleaning up the test environment after tests complete
    /// </summary>
    [OneTimeTearDown]
    public async Task TearDownAsync()
    {
        TestContext.Progress.WriteLine("Tearing down container.");
        await RunDockerComposeAsync("down");
    }

    private static async Task RunDockerComposeAsync(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{ComposeFile}\" {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"docker compose {arguments} failed: {error}");
        }
    }

    private static async Task WaitForEmulatorAsync(int timeoutSeconds = 60)
    {
        var startTime = Stopwatch.GetTimestamp();

        using var client = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync("http://localhost:5300/health");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TestContext.Progress.WriteLine($"Took {Stopwatch.GetElapsedTime(startTime).TotalSeconds:0.00} seconds to start Service Bus Container.");
                    return;
                }
            }
            catch { /* not ready yet */ }

            TestContext.Progress.WriteLine($"Waiting for container...({(int)Stopwatch.GetElapsedTime(startTime).TotalSeconds}s)");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        throw new TimeoutException($"Service Bus emulator did not become healthy within {timeoutSeconds} seconds.");
    }
}
