using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.ParametersAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParametersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        ParametersRules.RequiredParameter,
        ParametersRules.ParameterName,
        ParametersRules.ParameterPropertyTabAttribute,
        ParametersRules.ParameterUnknown,
        ParametersRules.ParametersOrder,
        GeneralRules.MetadataMissing
    ];

    private static readonly ImmutableArray<ExpectedParameter> ExpectedParameters =
    [
        new() { Type = "Input", Name = "input" },
        new() { Type = "Connection", Name = "connection" },
        new() { Type = "Options", Name = "options", Required = true },
        new() { Type = "CancellationToken", Name = "cancellationToken", Required = true, IsProperty = false }
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var tree = context.Compilation.SyntaxTrees.FirstOrDefault();
        if (tree is null)
            return;
        var taskMethods = context.Options.GetTaskMethods(tree, context.CancellationToken);
        context.RegisterSyntaxNodeAction(symbolContext => AnalyzeParameters(symbolContext, taskMethods),
            SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeParameters(SyntaxNodeAnalysisContext context, IImmutableList<TaskMethod>? taskMethods)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax) return;
        if (taskMethods is null || !taskMethods.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(GeneralRules.MetadataMissing, Location.None));
            return;
        }

        var taskMethod = taskMethods.FirstOrDefault(t => t.System == classSyntax.Identifier.Text);
        if (taskMethod is null) return;

        var method = classSyntax.Members.OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == taskMethod.Action);
        if (method is null) return;
        var parameters = method.ParameterList.Parameters.ToArray();

        //FT0007
        var foundExpectedParameters =
            ExpectedParameters.Where(ep => parameters.Any(p => p.Type?.ToString() == ep.Type)).ToArray();
        var missingRequiredParameters = ExpectedParameters.Where(p => p.Required).Except(foundExpectedParameters);
        foreach (var missingRequiredParameter in missingRequiredParameters)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(ParametersRules.RequiredParameter, method.Identifier.GetLocation(),
                    missingRequiredParameter.Type)
            );
        }

        var orderIndex = 0;
        var orderHandled = false;
        foreach (var parameter in parameters)
        {
            var parameterType = parameter.Type?.ToString();
            if (parameterType is null) continue;
            var matchedExpectedParameter =
                ExpectedParameters.FirstOrDefault(ep => ep.Type == parameterType);

            //FT0018
            if (matchedExpectedParameter is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(ParametersRules.ParameterUnknown, parameter.Identifier.GetLocation(),
                        parameterType)
                );
                continue;
            }

            //FT0008
            if (parameter.Identifier.Text != matchedExpectedParameter.Name)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(ParametersRules.ParameterName, parameter.Identifier.GetLocation(),
                        matchedExpectedParameter.Name)
                );
            }

            //FT0009
            if (matchedExpectedParameter.IsProperty)
            {
                var correctAttributes = parameter.AttributeLists
                    .Where(aList => aList.Attributes.Any(a => a.Name.ToString() == "PropertyTab"));
                if (!correctAttributes.Any())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(ParametersRules.ParameterPropertyTabAttribute,
                            parameter.Identifier.GetLocation())
                    );
                }
            }

            //FT0019
            if (!orderHandled && ExpectedParameters.Any(ep => ep.Type == parameterType))
            {
                var orderedParameter = ExpectedParameters.Skip(orderIndex)
                    .FirstOrDefault(ep => ep.Type == parameterType);
                if (orderedParameter is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(ParametersRules.ParametersOrder, method.Identifier.GetLocation())
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
