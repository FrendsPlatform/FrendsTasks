using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.ParametersAnalyzer;

public static class ParametersRules
{
    public static readonly DiagnosticDescriptor RequiredParameter =
        new("FT0007",
            "Missing a required parameter",
            "Task method is missing required parameter of type '{0}'",
            "Parameters",
            DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ParameterName =
        new("FT0008",
            "Recommended parameter name",
            "For consistency, the recommended name for this parameter is '{0}'",
            "Parameters",
            DiagnosticSeverity.Info, true);

    public static readonly DiagnosticDescriptor ParameterPropertyTabAttribute =
        new("FT0009",
            "Missing PropertyTab attribute",
            "Parameter should be attributed with 'System.ComponentModel.PropertyTabAttribute'",
            "Parameters",
            DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ParameterUnknown =
        new("FT0018",
            "Parameter is not recognized",
            "Parameter of type '{0}' is not recognized. Expected parameters are: 'Input', 'Connection', 'Options', 'CancellationToken'.",
            "Parameters",
            DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor ParametersOrder =
        new("FT0019",
            "Parameters are not in the correct order",
            "Parameters standard order is: 'Input', 'Connection', 'Options', 'CancellationToken'",
            "Parameters",
            DiagnosticSeverity.Warning, true);
}
