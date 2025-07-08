using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers;

// TODO: Rules should live with the analyzers responsible of them

public static class Rules
{
    public static readonly DiagnosticDescriptor TaskMethodOverloaded =
        new("FTA0001",
            "Task method is overloaded",
            "Task method should not be overloaded",
            "Correctness",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor TaskMethodRequiredParameter =
        new("FTA0002",
            "Missing a required parameter",
            "Task method is missing required parameter of type '{0}'",
            "Correctness",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor TaskMethodDeprecatedParameter =
        new("FTA0003",
            "Using a deprecated parameter",
            "Parameter of type '{0}' is deprecated. Use 'Input', 'Connection' or 'Options' instead.",
            "Correctness",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor TaskMethodParameterType =
        new("FTA0004",
            "Incorrect parameter type",
            "Parameter type should be '{0}' instead",
            "Correctness",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor TaskMethodParameterName =
        new("FTA0005",
            "Recommended parameter name",
            "For consistency, the recommended name for this parameter is '{0}'",
            "Correctness",
            DiagnosticSeverity.Info, true);

    public static readonly DiagnosticDescriptor TaskMethodPropertyTabAttribute =
        new("FTA0006",
            "Missing PropertyTab attribute",
            "Parameter should be attributed with 'System.ComponentModel.PropertyTabAttribute'",
            "Correctness",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor TaskMethodUnrecognizedParameter =
        new("FTA0007",
            "Parameter is not recognized",
            "Parameter of type '{0}' is not recognized. Expected parameters are: 'Input', 'Connection', 'Options', 'CancellationToken'.",
            "Correctness", DiagnosticSeverity.Warning, true);
}
