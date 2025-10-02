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
        context.RegisterCompilationStartAction(RegisterActions);
    }

    protected abstract void RegisterActions(CompilationStartAnalysisContext context);
}
