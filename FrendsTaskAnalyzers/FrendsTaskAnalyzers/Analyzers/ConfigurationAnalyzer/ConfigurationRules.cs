using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Analyzers.ConfigurationAnalyzer;

public static class ConfigurationRules
{
    public static readonly DiagnosticDescriptor ConfigurationMissing =
        new("FT0020",
            "Metadata is missing",
            "There is no metadata in the project",
            "General",
            DiagnosticSeverity.Warning, true,
            customTags: [WellKnownDiagnosticTags.CompilationEnd]
        );

}
