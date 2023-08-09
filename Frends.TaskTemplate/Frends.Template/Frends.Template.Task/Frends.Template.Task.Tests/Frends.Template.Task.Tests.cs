using Frends.Template.Task.Definitions;
using NUnit.Framework;
using System;

namespace Frends.Template.Task.Tests;

[TestFixture]
class TestClass
{
    /// <summary>
    /// You need to run Frends.Community.Echo.SetPaswordsEnv.ps1 before running unit test, or some other way set environment variables e.g. with GitHub Secrets.
    /// </summary>
    [Test]
    public void ThreeEchos()
    {
        var input = new Input
        {
            Content = Environment.GetEnvironmentVariable("EXAMPLE_ENVIROMENT_VARIABLE")
    };

        var options = new Options
        {
            Amount = 3,
            Delimiter = ", "
        };

        var ret = Template.Task(input, options, default);

        Assert.That(ret.Output, Is.EqualTo("foobar, foobar, foobar"));
    }
}
