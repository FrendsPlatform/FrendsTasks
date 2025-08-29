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
        // TODO: Ensure consistent behavior with normal reading of TaskMethods. No need to assign to TaskMethods.

        var tree = context.Compilation.SyntaxTrees.FirstOrDefault();
        TaskMethods = tree is not null
            ? context.Options.GetTaskMethods(tree, context.CancellationToken)
            : null;

        if (TaskMethods is null || !TaskMethods.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(ConfigurationRules.ConfigurationMissing, Location.None));
        }
    }
}
