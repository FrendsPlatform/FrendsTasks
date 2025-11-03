using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Analyzers.TypeAnalyzer;

public static class TypeRules
{
    public static readonly DiagnosticDescriptor RequiredPropertyMissing =
        new("FT0015",
            "Task parameter missing required property",
            "Parameter '{0}' is missing required property '{1}'",
            "Types",
            DiagnosticSeverity.Error, true,
            customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public static readonly DiagnosticDescriptor IncorrectPropertyDefaultValue =
        new("FT0016",
            "Task parameter property has incorrect default value",
            "Default value for property '{0}' should be '{1}'",
            "Types",
            DiagnosticSeverity.Error, true,
            customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public static readonly DiagnosticDescriptor ExposedThirdPartyType =
        new("FT0017",
            "Task exposes third-party types",
            "Property '{0}' exposes a third-party type",
            "Types",
            DiagnosticSeverity.Error,
            true);
}
