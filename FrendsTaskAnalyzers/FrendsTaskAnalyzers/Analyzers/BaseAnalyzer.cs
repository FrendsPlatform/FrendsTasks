using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers;

public abstract class BaseAnalyzer : DiagnosticAnalyzer
{
    protected IImmutableList<TaskMethod> TaskMethods = [];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(RegisterActions);
    }

    protected abstract void RegisterActions(CompilationStartAnalysisContext context);

#pragma warning disable RS1012 //actions registrations in overridden method
    /// <summary>
    /// Assigns TaskMethods from analyzer config to the TaskMethods property.
    /// Returns True if any TaskMethods where assigned. False otherwise.
    /// </summary>
    protected bool AssignTaskMethods(CompilationStartAnalysisContext context)
    {
        var syntaxTree = context.Compilation.SyntaxTrees.FirstOrDefault();
        if (syntaxTree is null) return false;
        TaskMethods = context.Options.GetTaskMethods(syntaxTree, context.CancellationToken) ?? [];
        return TaskMethods.Count != 0;
    }
#pragma warning restore RS1013
}
