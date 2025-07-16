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
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    protected virtual void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var tree = context.Compilation.SyntaxTrees.FirstOrDefault();
        if (tree is not null)
        {
            TaskMethods = context.Options.GetTaskMethods(tree, context.CancellationToken);
        }

        context.RegisterSyntaxNodeAction(symbolContext => AnalyzeBaseRules(symbolContext, TaskMethods),
            SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeBaseRules(SyntaxNodeAnalysisContext context, IImmutableList<TaskMethod>? taskMethods)
    {
        if (taskMethods is null || !taskMethods.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(BaseRules.ConfigurationMissing, Location.None));
        }
    }

    private static ImmutableArray<DiagnosticDescriptor> InitializeSupportedDiagnostics(
        ImmutableArray<DiagnosticDescriptor> additionalDiagnostics)
    {
        ImmutableArray<DiagnosticDescriptor> baseDiagnostics = [BaseRules.ConfigurationMissing];
        return baseDiagnostics.AddRange(additionalDiagnostics);
    }
}
