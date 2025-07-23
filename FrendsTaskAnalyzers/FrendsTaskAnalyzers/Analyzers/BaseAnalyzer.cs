using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers;

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
            TaskMethods = compilationContext.Compilation.SyntaxTrees
                .Select(tree => compilationContext.Options.GetTaskMethods(tree, compilationContext.CancellationToken))
                .FirstOrDefault(methods => methods?.Any() == true);
        });

        context.RegisterCompilationStartAction(RegisterActions);
    }

    protected abstract void RegisterActions(CompilationStartAnalysisContext context);

    private static ImmutableArray<DiagnosticDescriptor> InitializeSupportedDiagnostics(
        ImmutableArray<DiagnosticDescriptor> additionalDiagnostics)
    {
        ImmutableArray<DiagnosticDescriptor> baseDiagnostics = [];
        return baseDiagnostics.AddRange(additionalDiagnostics);
    }
}
