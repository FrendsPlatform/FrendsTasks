using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.ConfigurationAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConfigurationAnalyzer : BaseAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [ConfigurationRules.ConfigurationMissing];

#pragma warning disable RS1013 //other analyzers register more actions within this override method
    protected override void RegisterActions(CompilationStartAnalysisContext context)
        => context.RegisterCompilationEndAction(AnalyzeCompilationEnd);
#pragma warning restore RS1013


    private void AnalyzeCompilationEnd(CompilationAnalysisContext context)
    {
        var tree = context.Compilation.SyntaxTrees.FirstOrDefault();
        if (tree is not null)
            TaskMethods = context.Options.GetTaskMethods(tree, context.CancellationToken) ?? [];
        if (TaskMethods.Count == 0)
            context.ReportDiagnostic(Diagnostic.Create(ConfigurationRules.ConfigurationMissing, Location.None));
    }
}
