using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;
using Party.Echo.Execute.Definitions;

namespace Party.Echo.Execute.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    // TODO: Remove the container field, GlobalSetup and GlobalTeardown below if the task does not need
    // an external service for testing. Otherwise replace the image with the one your task needs
    // (e.g. "atmoz/sftp", "dperson/samba", "postgres") together with the matching port bindings and configuration.
    //
    // Testcontainers starts a real Docker container for the test run, so Docker must be running
    // on the machine that executes the tests. Use it instead of docker compose when you want the test
    // to be self-contained (the container is started and removed automatically, no manual "docker compose up").
    private IContainer container;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        container = new ContainerBuilder("hello-world:latest")
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
    public async Task GlobalTeardown()
    {
        if (container != null)
        {
            await container.DisposeAsync();
        }
    }

    [Test]
    public void ShouldRepeatContentWithDelimiter()
    {
        var input = new Input
        {
            Content = "foobar",
            Repeat = 3,
        };

        var connection = new Connection
        {
            ConnectionString = "Host=127.0.0.1;Port=12345",
        };

        var options = new Options
        {
            Delimiter = ", ",
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = null,
        };

        var result = Echo.Execute(input, connection, options, CancellationToken.None);

        Assert.That(result.Output, Is.EqualTo("foobar, foobar, foobar"));
    }
}
