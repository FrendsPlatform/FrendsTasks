using System.Collections.Immutable;
using System.Linq;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.NameAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NameAnalyzer : BaseAnalyzer
{
    protected override ImmutableArray<DiagnosticDescriptor> AdditionalDiagnostics =>
        [NameRules.NamespaceFormat, NameRules.TypeName, NameRules.MethodName];

    protected override void RegisterActions(CompilationStartAnalysisContext context)
    {
        var syntaxTree = context.Compilation.SyntaxTrees.FirstOrDefault();
        if (syntaxTree is null) return;

        var taskMethods = context.Options.GetTaskMethods(syntaxTree, context.CancellationToken);
        if (taskMethods is null) return;

        context.RegisterSymbolAction(symbolContext => AnalyzeMethods(symbolContext, taskMethods),
            SymbolKind.Method);

        context.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, taskMethods),
            SymbolKind.NamedType);

        context.RegisterSymbolAction(symbolContext => AnalyzeNamespace(symbolContext, taskMethods),
            SymbolKind.Namespace);
    }

    private static void AnalyzeMethods(SymbolAnalysisContext context, IImmutableList<TaskMethod> taskMethods)
    {
        if (context.Symbol is not IMethodSymbol symbol) return;

        var taskMethod = taskMethods.FirstOrDefault(t => t.Path == symbol.ToReferenceString());
        if (taskMethod?.Action is not { } action || symbol.Name == action) return;

        foreach (var location in symbol.Locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(NameRules.MethodName, location, action));
        }
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, IImmutableList<TaskMethod> taskMethods)
    {
        if (context.Symbol is not INamedTypeSymbol symbol) return;

        var taskMethod = taskMethods.FirstOrDefault(t =>
            symbol.GetMembers().OfType<IMethodSymbol>().Any(m => t.Path == m.ToReferenceString()));
        if (taskMethod?.System is not { } system || symbol.Name == system) return;

        foreach (var location in symbol.Locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(NameRules.TypeName, location, system));
        }
    }

    private static void AnalyzeNamespace(SymbolAnalysisContext context, IImmutableList<TaskMethod> taskMethods)
    {
        if (context.Symbol is not INamespaceSymbol symbol) return;

        var taskMethod = taskMethods.FirstOrDefault(t =>
            symbol.GetTypeMembers().Any(n =>
                n.GetMembers().OfType<IMethodSymbol>().Any(m => t.Path == m.ToReferenceString())));
        if (taskMethod is null) return;

        if (taskMethod.Vendor is not null && taskMethod.System is not null && taskMethod.Action is not null) return;

        foreach (var syntaxReference in symbol.ContainingNamespace.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax(context.CancellationToken);
            if (syntax is not BaseNamespaceDeclarationSyntax namespaceSyntax) continue;

            var location = namespaceSyntax.Name.GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(NameRules.NamespaceFormat, location));
        }
    }
}
