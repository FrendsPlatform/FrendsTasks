using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using FrendsTaskAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Moq;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.Extensions;

public class AnalyzerOptionsExtensionsTests
{
    private const string RootNamespaceKey = "build_property.rootnamespace";
    private const string TaskMethodsKey = "frends_task_analyzers.task_methods";

    private const string TaskMetadataFileName = "FrendsTaskMetadata.json";

    private static readonly SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText("");

    [Fact]
    public void GetTaskMethods_WithNoConfiguration_ShouldReturnNull()
    {
        var options = CreateAnalyzerOptions(null, null, null);
        var result = options.GetTaskMethods(SyntaxTree, CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public void GetTaskMethods_WithEditorConfig_ShouldReturnTaskMethods()
    {
        var options = CreateAnalyzerOptions("Frends.TestB.Execute",
            "Frends.TestA.Execute;Frends.TestB.Execute.TestB.Execute", null);

        var taskMethods = options.GetTaskMethods(SyntaxTree, CancellationToken.None);

        Assert.Equal(2, taskMethods.Count);

        Assert.Equal("Frends.TestA.Execute", taskMethods[0].Path);
        Assert.Null(taskMethods[0].Vendor);
        Assert.Null(taskMethods[0].System);
        Assert.Null(taskMethods[0].Action);

        Assert.Equal("Frends.TestB.Execute.TestB.Execute", taskMethods[1].Path);
        Assert.Equal("Frends", taskMethods[1].Vendor);
        Assert.Equal("TestB", taskMethods[1].System);
        Assert.Equal("Execute", taskMethods[1].Action);
    }

    [Fact]
    public void GetTaskMethods_WithJsonFile_ShouldReturnTaskMethods()
    {
        var metadata = new JsonObject
        {
            ["Tasks"] = new JsonArray(
                new JsonObject { ["TaskMethod"] = "Frends.TestA.Execute" },
                new JsonObject { ["TaskMethod"] = "Frends.TestB.Execute.TestB.Execute" })
        }.ToJsonString();

        var options = CreateAnalyzerOptions(null, null, metadata);

        var taskMethods = options.GetTaskMethods(SyntaxTree, CancellationToken.None);

        Assert.Equal(2, taskMethods.Count);

        Assert.Equal("Frends.TestA.Execute", taskMethods[0].Path);
        Assert.Null(taskMethods[0].Vendor);
        Assert.Null(taskMethods[0].System);
        Assert.Null(taskMethods[0].Action);

        Assert.Equal("Frends.TestB.Execute.TestB.Execute", taskMethods[1].Path);
        Assert.Equal("Frends", taskMethods[1].Vendor);
        Assert.Equal("TestB", taskMethods[1].System);
        Assert.Equal("Execute", taskMethods[1].Action);
    }

    [Fact]
    public void GetTaskMethods_WithEditorConfigAndJsonFile_ShouldReturnTaskMethodsFromEditorConfig()
    {
        var metadata = new JsonObject
        {
            ["Tasks"] = new JsonArray(
                new JsonObject { ["TaskMethod"] = "Frends.TestA.Execute" },
                new JsonObject { ["TaskMethod"] = "Frends.TestB.Execute.TestB.Execute" })
        }.ToJsonString();

        var options = CreateAnalyzerOptions("Frends.TestC.Execute",
            "Frends.TestC.Execute;Frends.TestD.Execute.TestD.Execute", metadata);

        var taskMethods = options.GetTaskMethods(SyntaxTree, CancellationToken.None);

        Assert.Equal(2, taskMethods.Count);

        Assert.Equal("Frends.TestC.Execute", taskMethods[0].Path);
        Assert.Null(taskMethods[0].Vendor);
        Assert.Null(taskMethods[0].System);
        Assert.Null(taskMethods[0].Action);

        Assert.Equal("Frends.TestD.Execute.TestD.Execute", taskMethods[1].Path);
        Assert.Equal("Frends", taskMethods[1].Vendor);
        Assert.Equal("TestD", taskMethods[1].System);
        Assert.Equal("Execute", taskMethods[1].Action);
    }

    private static AnalyzerOptions CreateAnalyzerOptions(string? rootNamespace, string? taskMethods, string? metadata)
    {
        ImmutableArray<AdditionalText> additionalFiles = [];

        if (metadata is not null)
        {
            var additionalFile = new Mock<AdditionalText>();
            additionalFile.Setup(a => a.Path).Returns(TaskMetadataFileName);
            additionalFile.Setup(a => a.GetText(CancellationToken.None))
                .Returns(SourceText.From(metadata, Encoding.UTF8));

            additionalFiles = [additionalFile.Object];
        }

        var options = new Mock<AnalyzerConfigOptions>();
        options.Setup(o => o.TryGetValue(RootNamespaceKey, out rootNamespace)).Returns(rootNamespace is not null);
        options.Setup(o => o.TryGetValue(TaskMethodsKey, out taskMethods)).Returns(taskMethods is not null);

        var provider = new Mock<AnalyzerConfigOptionsProvider>();
        provider.Setup(p => p.GlobalOptions).Returns(options.Object);
        provider.Setup(p => p.GetOptions(SyntaxTree)).Returns(options.Object);

        return new AnalyzerOptions(additionalFiles, provider.Object);
    }
}
