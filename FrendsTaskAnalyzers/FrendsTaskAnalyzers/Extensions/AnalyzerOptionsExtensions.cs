using System;
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

    private const string KeyPrefix = "frends_task_analyzers";
    private const string TaskMethodsKey = $"{KeyPrefix}.task_methods";

    public static IImmutableList<TaskMetadata> GetTaskMetadata(
        this AnalyzerOptions options, SyntaxTree tree, CancellationToken cancellationToken)
    {
        if (TaskMetadataFromJsonFile(options, cancellationToken) is { } jsonTaskMetadata)
            return jsonTaskMetadata;

        if (TaskMetadataFromConfig(options, tree) is { } configTaskMetadata)
            return configTaskMetadata;

        throw new InvalidOperationException("Task metadata could not be found.");
    }

    private static IImmutableList<TaskMetadata>? TaskMetadataFromJsonFile(
        AnalyzerOptions options, CancellationToken cancellationToken)
    {
        var text = options.AdditionalFiles
            .FirstOrDefault(f => Path.GetFileName(f.Path) == TaskMetadataFileName)
            ?.GetText(cancellationToken);

        if (text == null)
            return null;

        using var document = JsonDocument.Parse(text.ToString());
        var root = document.RootElement;


        if (!root.TryGetProperty("Tasks", out var tasks) || tasks.ValueKind != JsonValueKind.Array)
            return null;

        var taskMethods = tasks.EnumerateArray()
            .Select(t => t.TryGetProperty("TaskMethod", out var taskMethod) ? taskMethod.GetString() : null);

        return ParseTaskMethods(taskMethods);
    }

    private static IImmutableList<TaskMetadata>? TaskMetadataFromConfig(AnalyzerOptions options, SyntaxTree tree)
    {
        var config = options.AnalyzerConfigOptionsProvider.GetOptions(tree);

        if (!config.TryGetValue(TaskMethodsKey, out var taskMethods) || string.IsNullOrWhiteSpace(taskMethods))
            return null;

        return ParseTaskMethods(taskMethods.Split(';'));
    }

    private static IImmutableList<TaskMetadata> ParseTaskMethods(IEnumerable<string?> taskMethods) => taskMethods
        .Select(taskMethod =>
        {
            if (string.IsNullOrWhiteSpace(taskMethod))
                return null;

            var parts = taskMethod!.Split('.');
            return parts.Length >= 3
                ? new TaskMetadata(parts[0], parts[1], parts[2])
                : null;
        })
        .Where(t => t != null)
        .ToImmutableList()!;
}
