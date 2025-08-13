using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.ParametersAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParametersAnalyzer : BaseAnalyzer
{
    protected override ImmutableArray<DiagnosticDescriptor> AdditionalDiagnostics { get; } =
    [
        ParametersRules.RequiredParameter,
        ParametersRules.ParameterName,
        ParametersRules.ParameterPropertyTabAttribute,
        ParametersRules.ParameterUnknown,
        ParametersRules.ParametersOrder,
    ];

    private static readonly ImmutableArray<ExpectedParameter> ExpectedParameters =
    [
        new() { Type = "Input", Name = "input", Required = true },
        new() { Type = "Connection", Name = "connection" },
        new() { Type = "Options", Name = "options", Required = true },
        new() { Type = "CancellationToken", Name = "cancellationToken", Required = true, IsProperty = false }
    ];

    protected override void RegisterActions(CompilationStartAnalysisContext context)
        => context.RegisterSyntaxNodeAction(AnalyzeParameters, SyntaxKind.MethodDeclaration);

    private void AnalyzeParameters(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodSyntax) return;
        var symbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);
        if (symbol is null) return;

        if (TaskMethods?.Any(t => t.Path == symbol.ToReferenceString()) != true) return;

        var parameters = symbol.Parameters.ToArray();

        //FT0007
        var foundExpectedParameters =
            ExpectedParameters.Where(ep => parameters.Any(p => p.Type.Name == ep.Type)).ToArray();
        var missingRequiredParameters = ExpectedParameters.Where(p => p.Required).Except(foundExpectedParameters);
        foreach (var missingRequiredParameter in missingRequiredParameters)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(ParametersRules.RequiredParameter, symbol?.Locations.FirstOrDefault(),
                    missingRequiredParameter.Type)
            );
        }

        var propertyTabAttributeSymbol =
            context.Compilation.GetTypeByMetadataName("System.ComponentModel.PropertyTabAttribute");

        var orderIndex = 0;
        var orderHandled = false;
        foreach (var parameter in parameters)
        {
            var matchedExpectedParameter =
                ExpectedParameters.FirstOrDefault(ep => ep.Type == parameter.Type.Name);

            //FT0018
            if (matchedExpectedParameter is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(ParametersRules.ParameterUnknown, parameter.Locations.FirstOrDefault(),
                        parameter.Type.Name)
                );
                continue;
            }

            //FT0008
            if (parameter.Name != matchedExpectedParameter.Name)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(ParametersRules.ParameterName, parameter.Locations.FirstOrDefault(),
                        matchedExpectedParameter.Name)
                );
            }

            //FT0009
            if (matchedExpectedParameter.IsProperty)
            {
                if (propertyTabAttributeSymbol is null) continue;

                var attributes = parameter.GetAttributes().ToList();
                var hasPropertyTabAttribute = attributes.Any(a =>
                {
                    var attributeSymbol = a.AttributeClass;
                    return attributeSymbol is not null &&
                           attributeSymbol.Equals(propertyTabAttributeSymbol, SymbolEqualityComparer.Default);
                });

                if (!hasPropertyTabAttribute)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(ParametersRules.ParameterPropertyTabAttribute,
                            parameter.Locations.FirstOrDefault())
                    );
                }
            }

            //FT0019
            if (!orderHandled)
            {
                var orderedParameter = ExpectedParameters.Skip(orderIndex)
                    .FirstOrDefault(ep => ep.Type == parameter.Type.Name);
                if (orderedParameter is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(ParametersRules.ParametersOrder, symbol?.Locations.FirstOrDefault())
                    );
                    orderHandled = true;
                }
                else
                {
                    orderIndex = ExpectedParameters.IndexOf(orderedParameter) + 1;
                }
            }
        }
    }
}
