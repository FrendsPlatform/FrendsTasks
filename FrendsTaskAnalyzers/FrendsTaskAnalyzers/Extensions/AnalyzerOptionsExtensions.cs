using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Extensions;

public static class AnalyzerOptionsExtensions
{
    private const string TaskMetadataFileName = "FrendsTaskMetadata.json";

    private const string RootNamespaceKey = "build_property.rootnamespace";
    private const string KeyPrefix = "frends_task_analyzers";
    private const string TaskMethodsKey = $"{KeyPrefix}.task_methods";

    public static IImmutableList<TaskMethod>? GetTaskMethods(
        this AnalyzerOptions options,
        SyntaxTree tree,
        CancellationToken cancellationToken)
    {
        options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(RootNamespaceKey, out var rootNamespace);

        if (TaskMethodsFromConfig(options, rootNamespace, tree) is { } configTaskMethods)
            return configTaskMethods;

        if (TaskMethodsFromJsonFile(options, rootNamespace, cancellationToken) is { } jsonTaskMethods)
            return jsonTaskMethods;

        return null;
    }

    private static IImmutableList<TaskMethod>? TaskMethodsFromJsonFile(
        AnalyzerOptions options,
        string? rootNamespace,
        CancellationToken cancellationToken)
    {
        var text = options.AdditionalFiles
            .FirstOrDefault(f => Path.GetFileName(f.Path) == TaskMetadataFileName)
            ?.GetText(cancellationToken);
        if (text == null) return null;

        using var document = JsonDocument.Parse(text.ToString());
        var root = document.RootElement;


        if (!root.TryGetProperty("Tasks", out var tasks) || tasks.ValueKind != JsonValueKind.Array)
            return null;

        var taskMethods = tasks.EnumerateArray()
            .Select(t => t.TryGetProperty("TaskMethod", out var taskMethod) ? taskMethod.GetString() : null);

        return ParseTaskMethods(taskMethods, rootNamespace);
    }

    private static IImmutableList<TaskMethod>? TaskMethodsFromConfig(
        AnalyzerOptions options,
        string? rootNamespace,
        SyntaxTree tree)
    {
        var config = options.AnalyzerConfigOptionsProvider.GetOptions(tree);

        if (!config.TryGetValue(TaskMethodsKey, out var taskMethods) || string.IsNullOrWhiteSpace(taskMethods))
            return null;

        return ParseTaskMethods(taskMethods.Split(';'), rootNamespace);
    }

    private static IImmutableList<TaskMethod> ParseTaskMethods(
        IEnumerable<string?> taskMethods,
        string? rootNamespace) => taskMethods
        .Select(taskMethod =>
            !string.IsNullOrWhiteSpace(taskMethod) ? TaskMethod.Parse(taskMethod!, rootNamespace) : null)
        .Where(t => t != null)
        .ToImmutableList()!;
}
