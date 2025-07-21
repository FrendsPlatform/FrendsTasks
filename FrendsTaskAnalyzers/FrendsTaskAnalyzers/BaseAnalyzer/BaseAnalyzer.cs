using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.BaseAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public abstract class BaseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        InitializeSupportedDiagnostics(AdditionalDiagnostics);

    protected abstract ImmutableArray<DiagnosticDescriptor> AdditionalDiagnostics { get; }

    protected IImmutableList<TaskMethod>? TaskMethods;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(compilationContext =>
        {
            var tree = compilationContext.Compilation.SyntaxTrees.FirstOrDefault();
            TaskMethods = tree is not null
                ? compilationContext.Options.GetTaskMethods(tree, compilationContext.CancellationToken)
                : null;

            if (TaskMethods is null || !TaskMethods.Any())
            {
                compilationContext.ReportDiagnostic(
                    Diagnostic.Create(BaseRules.ConfigurationMissing, Location.None));
            }
        });

        context.RegisterCompilationStartAction(RegisterActions);
    }

    protected abstract void RegisterActions(CompilationStartAnalysisContext context);

    private static ImmutableArray<DiagnosticDescriptor> InitializeSupportedDiagnostics(
        ImmutableArray<DiagnosticDescriptor> additionalDiagnostics)
    {
        ImmutableArray<DiagnosticDescriptor> baseDiagnostics = [BaseRules.ConfigurationMissing];
        return baseDiagnostics.AddRange(additionalDiagnostics);
    }
}
