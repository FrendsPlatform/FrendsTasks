using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TaskMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<ExpectedParameter> ExpectedParameters =
    [
        new("Input", "input", true),
        new("Connection", "connection", false),
        new("Options", "options", false),
        new("CancellationToken", "cancellationToken", true)
    ];

    private static readonly ImmutableArray<string> DeprecatedParameterTypes = ["Source", "Destination"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Rules.TaskMethodOverloaded,
        Rules.TaskMethodRequiredParameter,
        Rules.TaskMethodDeprecatedParameter,
        Rules.TaskMethodParameterType,
        Rules.TaskMethodParameterName,
        Rules.TaskMethodPropertyTabAttribute,
        Rules.TaskMethodUnrecognizedParameter
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax)
            return;

        var taskMetadataFiltered = context.Options
            .GetTaskMethods(context.Node.SyntaxTree, context.CancellationToken)
            .Where(t => t.System == classSyntax.Identifier.Text);

        // TODO: We should handle the case of multiple tasks in the same class. (Run analysis in a loop?)
        var taskMetadata = taskMetadataFiltered.FirstOrDefault();

        var taskAction = taskMetadata?.Action;
        if (taskAction == null)
            return;

        var matchedTaskMethods = classSyntax.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text == taskAction).ToArray();

        if (matchedTaskMethods.Length == 0)
            return;

        if (matchedTaskMethods.Length > 1)
        {
            foreach (var method in matchedTaskMethods)
            {
                var diagnostic = Diagnostic.Create(Rules.TaskMethodOverloaded, method.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            return;
        }

        var taskMethod = matchedTaskMethods.Single();
        var parameters = taskMethod.ParameterList.Parameters;

        var requiredParameters = ExpectedParameters.Where(p => p.Required);

        foreach (var requiredParameter in requiredParameters)
        {
            if (parameters.Any(p =>
                    p.Type?.ToString() == requiredParameter.Type || p.Identifier.Text == requiredParameter.Name))
                continue;

            var diagnostic = Diagnostic.Create(Rules.TaskMethodRequiredParameter, taskMethod.Identifier.GetLocation(),
                requiredParameter.Type);
            context.ReportDiagnostic(diagnostic);
        }

        foreach (var parameter in parameters)
        {
            Diagnostic? diagnostic;

            if (parameter.Type == null)
                continue;

            var type = parameter.Type.ToString();
            var name = parameter.Identifier.Text;

            var expectedParameter = ExpectedParameters.FirstOrDefault(p => type == p.Type || name == p.Name);
            if (expectedParameter != null)
            {
                if (type != expectedParameter.Type)
                {
                    diagnostic = Diagnostic.Create(Rules.TaskMethodParameterType,
                        parameter.Type.GetLocation(), expectedParameter.Type);
                    context.ReportDiagnostic(diagnostic);
                }

                if (name != expectedParameter.Name)
                {
                    diagnostic = Diagnostic.Create(Rules.TaskMethodParameterName,
                        parameter.Identifier.GetLocation(), expectedParameter.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            else if (DeprecatedParameterTypes.Contains(type))
            {
                diagnostic = Diagnostic.Create(Rules.TaskMethodDeprecatedParameter,
                    parameter.Type.GetLocation(), type);
                context.ReportDiagnostic(diagnostic);
            }
            else
            {
                diagnostic = Diagnostic.Create(Rules.TaskMethodUnrecognizedParameter,
                    parameter.Type.GetLocation(), type);
                context.ReportDiagnostic(diagnostic);
            }

            // TODO: Verify attribute type instead of name
            if (type == "CancellationToken" ||
                parameter.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == "PropertyTab")))
                continue;

            diagnostic = Diagnostic.Create(Rules.TaskMethodPropertyTabAttribute,
                parameter.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        // TODO: Check if parameters are in correct order
    }

    private class ExpectedParameter(string type, string name, bool required)
    {
        public string Type { get; } = type;
        public string Name { get; } = name;
        public bool Required { get; } = required;
    }
}
