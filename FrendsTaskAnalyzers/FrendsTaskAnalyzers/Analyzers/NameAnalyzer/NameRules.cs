using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Analyzers.NameAnalyzer;

public static class NameRules
{
    public static readonly DiagnosticDescriptor NamespaceFormat =
        new("FT0001",
            "Namespace does not follow the standard format",
            "Standard namespace format is 'Vendor.System.Action'",
            "Naming",
            DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor TypeName =
        new("FT0002",
            "Type should match the task system",
            "Type name should be '{0}'",
            "Naming",
            DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MethodName =
        new("FT0003",
            "Method should match the task action",
            "Method name should be '{0}'",
            "Naming",
            DiagnosticSeverity.Error, true);
}
