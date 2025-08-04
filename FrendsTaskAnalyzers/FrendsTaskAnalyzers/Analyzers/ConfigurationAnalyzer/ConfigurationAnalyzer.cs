using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.ConfigurationAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConfigurationAnalyzer : BaseAnalyzer
{
    protected override ImmutableArray<DiagnosticDescriptor> AdditionalDiagnostics =>
        [ConfigurationRules.ConfigurationMissing];

#pragma warning disable RS1013 //other analyzers register more actions within this override method
    protected override void RegisterActions(CompilationStartAnalysisContext context)
        => context.RegisterCompilationEndAction(AnalyzeTaskMethods);
#pragma warning restore RS1013


    private void AnalyzeTaskMethods(CompilationAnalysisContext context)
    {
        var tree = context.Compilation.SyntaxTrees.FirstOrDefault();
        TaskMethods = tree is not null
            ? context.Options.GetTaskMethods(tree, context.CancellationToken)
            : null;

        if (TaskMethods is null || !TaskMethods.Any())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(ConfigurationRules.ConfigurationMissing, Location.None));
        }
    }
}
