using System;
using FrendsTaskAnalyzers.Models;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.Models;

public class TaskMethodTests
{
    [Theory]
    [InlineData("Execute", null)]
    [InlineData("Test.Execute", null)]
    [InlineData("Execute", "Frends.Test.Execute")]
    [InlineData("Test.Execute", "Frends.Test.Execute")]
    [InlineData(".Frends.Test.Execute", null)]
    [InlineData("Frends.Test.Execute.", null)]
    [InlineData(".Frends.Test.Execute", "Frends.Test.Execute")]
    [InlineData("Frends.Test.Execute.", "Frends.Test.Execute")]
    public void Parse_WithInvalidPath_ShouldThrow(string path, string? rootNamespace) =>
        Assert.Throws<ArgumentException>(() => TaskMethod.Parse(path, rootNamespace));

    [Theory]
    [InlineData("Frends.Test.Execute", ".Frends.Test.Execute")]
    [InlineData("Frends.Test.Execute", "Frends.Test.Execute.")]
    public void Parse_WithInvalidRootNamespace_ShouldThrow(string path, string? rootNamespace) =>
        Assert.Throws<ArgumentException>(() => TaskMethod.Parse(path, rootNamespace));

    [Theory]
    [InlineData("Frends.Test.Execute.Test.Execute", null, "Frends", "Test", "Execute")]
    [InlineData("Frends.Test.Execute.Test.Actions.Execute", null, "Frends", "Test", "Execute")]
    [InlineData("Frends.Test.Execute.Test.Execute", "Frends.Test.Execute", "Frends", "Test", "Execute")]
    [InlineData("Frends.Test.Execute.Test.Actions.Execute", "Frends.Test.Execute", "Frends", "Test", "Execute")]
    public void Parse_WithValidPathAndRootNamespace_ShouldReturnExpectedComponents(
        string path, string? rootNamespace, string vendor, string system, string action)
    {
        var taskMethod = TaskMethod.Parse(path, rootNamespace);
        Assert.Equal(path, taskMethod.Path);
        Assert.Equal(vendor, taskMethod.Vendor);
        Assert.Equal(system, taskMethod.System);
        Assert.Equal(action, taskMethod.Action);
    }

    [Theory]
    [InlineData("Frends.Test.Execute", null)]
    [InlineData("Frends.Test.Execute", "Frends")]
    [InlineData("Frends.TestA.Execute", "Frends.TestB.Execute")]
    public void Parse_WithInvalidPathAndRootNamespace_ShouldReturnNoComponents(string path, string? rootNamespace)
    {
        var taskMethod = TaskMethod.Parse(path, rootNamespace);
        Assert.Equal(path, taskMethod.Path);
        Assert.Null(taskMethod.Vendor);
        Assert.Null(taskMethod.System);
        Assert.Null(taskMethod.Action);
    }
}
