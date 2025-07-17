using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.NameAnalyzer;

public static class NameRules
{
    public static readonly DiagnosticDescriptor NamespaceRule =
        new("FT0001",
            "Namespace does not follow the standard format",
            "Standard namespace format is 'Vendor.System.Action'",
            "Naming",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor TypeRule =
        new("FT0002",
            "Type should match the task system",
            "Type name should be '{0}'",
            "Naming",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor MethodRule =
        new("FT0003",
            "Method should match the task action",
            "Method name should be '{0}'",
            "Naming",
            DiagnosticSeverity.Warning, true);

}
