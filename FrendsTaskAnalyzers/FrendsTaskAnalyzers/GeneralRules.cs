using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers;

public static class GeneralRules
{
    public static readonly DiagnosticDescriptor MetadataMissing =
        new("FT0020",
            "Metadata is missing",
            "There is no metadata in the project",
            "General",
            DiagnosticSeverity.Warning, true);
}
