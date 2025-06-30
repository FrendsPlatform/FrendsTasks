using System;
using System.Linq;

namespace FrendsTaskAnalyzers.Models;

public class TaskMethod
{
    private TaskMethod(string path, string? vendor = null, string? system = null, string? action = null)
    {
        Path = path;
        Vendor = vendor;
        System = system;
        Action = action;
    }

    public string Path { get; }

    public string? Vendor { get; }

    public string? System { get; }

    public string? Action { get; }

    public static TaskMethod Parse(string path, string? rootNamespace)
    {
        var parts = path.Split('.');
        if (path.StartsWith(".") || path.EndsWith(".") || parts.Length < 3)
            throw new ArgumentException(
                "Invalid task method path, must be at least three parts and not start or end with a dot.",
                nameof(path));

        if (rootNamespace != null && (rootNamespace.StartsWith(".") || rootNamespace.EndsWith(".")))
            throw new ArgumentException("Invalid root namespace, must not start or end with a dot.",
                nameof(rootNamespace));

        if (parts.Length < 5)
            return new TaskMethod(path);

        if (!string.IsNullOrEmpty(rootNamespace) && path.StartsWith(rootNamespace + "."))
        {
            var rootNamespaceParts = rootNamespace.Count(c => c == '.') + 1;
            if (rootNamespaceParts != 3)
                return new TaskMethod(path);
        }

        var vendor = parts[0];
        var system = parts[1];
        var action = parts[2];
        return new TaskMethod(path, vendor, system, action);
    }
}
