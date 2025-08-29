using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers;

public abstract class BaseAnalyzer : DiagnosticAnalyzer
{
    protected IImmutableList<TaskMethod>? TaskMethods;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(ctx =>
        {
            TaskMethods = ctx.Compilation.SyntaxTrees
                .Select(tree => ctx.Options.GetTaskMethods(tree, ctx.CancellationToken))
                .FirstOrDefault(methods => methods?.Any() == true);
        });

        context.RegisterCompilationStartAction(RegisterActions);
    }

    protected abstract void RegisterActions(CompilationStartAnalysisContext context);
}
