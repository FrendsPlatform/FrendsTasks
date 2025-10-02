using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Analyzers.DocumentationAnalyzer;

public static class DocumentationRules
{
    public static readonly DiagnosticDescriptor DocumentationLinkMissing =
        new("FT0012",
            "Documentation link is missing",
            "Missing the task '[Documentation]' link",
            "Documentation",
            DiagnosticSeverity.Error, true
        );

    public static readonly DiagnosticDescriptor UnsupportedTagsUsed =
        new("FT0013",
            "Documentation is using unsupported tags",
            "Task documentation should not use tags like cfer and similar",
            "Documentation",
            DiagnosticSeverity.Error, true
        );

    public static readonly DiagnosticDescriptor RequiredTagsMissing =
        new("FT0014",
            "Documentation required tags are missing",
            "Missing a required documentation tag '{0}'",
            "Documentation",
            DiagnosticSeverity.Error, true
        );

    public static readonly DiagnosticDescriptor DocumentationInvalid =
        new("FT0021",
            "Documentation Xml is invalid",
            "Documentation Xml is not parsable",
            "Documentation",
            DiagnosticSeverity.Error, true
        );
}
