using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Analyzers.StructureAnalyzer;
public static class StructureRules
{
    public static readonly DiagnosticDescriptor ClassShouldBeStaticRule = new(
       "FT0004",
       "Task class should be static",
       "Task class '{0}' should be static",
       "Structure",
       DiagnosticSeverity.Warning,
       true);

    public static readonly DiagnosticDescriptor MethodShouldBeStaticRule = new(
        "FT0005",
        "Task method must be static",
        "Task method '{0}' must be static",
        "Structure",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor MethodOverloadNotAllowedRule = new(
        "FT0006",
        "Task method overloading is not allowed",
        "Task method '{0}' cannot be overloaded",
        "Structure",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor ReturnTypeIncorrectRule = new(
        "FT0010",
        "Task method return type incorrect",
        "Task return type should be '{0}'",
        "Structure",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor ReturnTypeMissingPropertiesRule = new(
        "FT0011",
        "Task return type missing required properties",
        "Class should include a '{0}' property",
        "Structure",
        DiagnosticSeverity.Warning,
        true);
}
