using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.NameAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NameAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [NameRules.NamespaceRule, NameRules.TypeRule, NameRules.MethodRule, GeneralRules.MetadataMissing];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var tree = context.Compilation.SyntaxTrees.FirstOrDefault();
        if (tree is null)
            return;

        var taskMethods = context.Options.GetTaskMethods(tree, context.CancellationToken);

        context.RegisterSymbolAction(symbolContext => AnalyzeTaskMethods(symbolContext, taskMethods),
            SymbolKind.Method);
    }

    private static void AnalyzeTaskMethods(SymbolAnalysisContext context, IImmutableList<TaskMethod>? taskMethods)
    {
        if (context.Symbol is not IMethodSymbol symbol)
            return;
        if (taskMethods is null || !taskMethods.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(GeneralRules.MetadataMissing, Location.None));
            return;
        }

        var taskMethod = taskMethods.FirstOrDefault(t => t.Path == symbol.ToReferenceString());
        if (taskMethod is null)
            return;

        if (taskMethod.Action is { } action && symbol.Name != action)
        {
            foreach (var location in symbol.Locations)
            {
                var diagnostic = Diagnostic.Create(NameRules.MethodRule, location, action);
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (taskMethod.System is { } system && symbol.ContainingType.Name != system)
        {
            foreach (var location in symbol.ContainingType.Locations)
            {
                var diagnostic = Diagnostic.Create(NameRules.TypeRule, location, system);
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (taskMethod.Vendor is null)
        {
            foreach (var syntaxReference in symbol.ContainingNamespace.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is not BaseNamespaceDeclarationSyntax namespaceSyntax)
                    continue;

                var location = namespaceSyntax.Name.GetLocation();
                var diagnostic = Diagnostic.Create(NameRules.NamespaceRule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
