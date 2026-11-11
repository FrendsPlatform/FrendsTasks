using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace Party.Echo.Execute.Tests;

// TODO: Remove this file if the task does not need an external service for testing.
// Otherwise replace the image with the one your task needs (e.g. "atmoz/sftp", "dperson/samba", "postgres")
// together with the matching port bindings and configuration. "hello-world" is only a placeholder,
// it exits immediately.
//
// Testcontainers starts a real Docker container for the test run, so Docker must be running
// on the machine that executes the tests. Use it instead of docker compose when you want the test
// to be self-contained (the container is started and removed automatically, no manual "docker compose up").
[SetUpFixture]
public sealed class SetupContainer
{
    private const string Image = "hello-world:latest";
    private IContainer container;

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        container = new ContainerBuilder(Image)
            .WithName($"test-container-{Guid.NewGuid()}")

            // .WithPortBinding(445, 445)                      // expose a port if the service needs it
            // .WithBindMount(testFilesPath, "/share")         // mount local files into the container
            // .WithEnvironment("KEY", "value")                // pass environment variables
            // .WithCommand("-u", "user;pass")                 // pass command line arguments
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(445)) // wait until the service is ready
            .Build();

        await container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task StopAsync()
    {
        if (container != null)
            await container.DisposeAsync();
    }
}
