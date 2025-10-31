using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Analyzers.ConfigurationAnalyzer;

public static class ConfigurationRules
{
    public static readonly DiagnosticDescriptor ConfigurationMissing =
        new("FT0020",
            "Missing analyzer configuration",
            "Project is missing analyzer configuration",
            "Configuration",
            DiagnosticSeverity.Error, true,
            customTags: [WellKnownDiagnosticTags.CompilationEnd]);
}
