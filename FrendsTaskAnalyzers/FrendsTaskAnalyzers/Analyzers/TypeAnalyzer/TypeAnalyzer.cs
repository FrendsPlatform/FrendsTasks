using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.TypeAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeAnalyzer : BaseAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        TypeRules.RequiredPropertyMissing,
        TypeRules.IncorrectPropertyDefaultValue,
        TypeRules.ExposedThirdPartyType
    ];

    protected override void RegisterActions(CompilationStartAnalysisContext context)
    {
        var taskMethods = context.Compilation.SyntaxTrees
            .Select(tree => context.Options.GetTaskMethods(tree, context.CancellationToken))
            .FirstOrDefault(t => t?.Any() == true);
        if (taskMethods is null) return;

        var types = FindTypesForAnalysis(context.Compilation, taskMethods);

        context.RegisterSymbolAction(ctx => AnalyzeNamedTypes(ctx, types), SymbolKind.NamedType);
        // context.RegisterSymbolAction(ctx => AnalyzeMethods(ctx, TaskMethods), SymbolKind.Method);
    }

    private static ImmutableDictionary<INamedTypeSymbol, ImmutableArray<(TaskMethod TaskMethod, TaskCategory Category)>>
        FindTypesForAnalysis(Compilation compilation, IImmutableList<TaskMethod> taskMethods) =>
        compilation.GlobalNamespace
            .GetMembers()
            .OfType<INamedTypeSymbol>()
            .SelectMany(t => t.GetMembers().OfType<IMethodSymbol>())
            .SelectMany(m =>
            {
                var taskMethod = taskMethods.FirstOrDefault(t => t.Path == m.ToReferenceString());
                if (taskMethod is null) return [];

                var taskCategory = m.GetTaskCategory(compilation);

                var types = m.Parameters.Select(p => p.Type).OfType<INamedTypeSymbol>().ToImmutableArray();
                if (m.ReturnType is INamedTypeSymbol returnTypeSymbol)
                    types = types.Add(returnTypeSymbol);

                return types.Select(t => (Type: t, TaskMethod: taskMethod, Category: taskCategory));
            })
            .GroupBy(x => x.Type, NamedTypeSymbolEqualityComparer.Instance)
            .ToImmutableDictionary(
                g => g.Key,
                g => g.Select(x => (x.TaskMethod, x.Category)).ToImmutableArray(),
                NamedTypeSymbolEqualityComparer.Instance);

    private static void AnalyzeNamedTypes(
        SymbolAnalysisContext context,
        ImmutableDictionary<INamedTypeSymbol, ImmutableArray<(TaskMethod TaskMethod, TaskCategory Category)>> types)
    {
        if (context.Symbol is not INamedTypeSymbol symbol) return;
        if (!types.TryGetValue(symbol, out var array)) return;

        foreach (var (taskMethod, category) in array)
        {
            CheckRequiredProperties(context, symbol, taskMethod, category);
        }
    }

    private static void AnalyzeMethods(
        SymbolAnalysisContext context,
        IImmutableList<TaskMethod> taskMethods,
        ConcurrentDictionary<INamedTypeSymbol, ImmutableArray<(TaskCategory, TaskMethod)>> types)
    {
        if (context.Symbol is not IMethodSymbol symbol) return;

        var taskMethod = taskMethods.FirstOrDefault(t => t.Path == symbol.ToReferenceString());
        if (taskMethod is null) return;

        var taskCategory = symbol.GetTaskCategory(context.Compilation);

        var parameterTypeSymbols = symbol.Parameters.Select(p => p.Type).OfType<INamedTypeSymbol>();

        foreach (var parameterTypeSymbol in parameterTypeSymbols)
        {
            types.AddOrUpdate(
                parameterTypeSymbol,
                _ => [(taskCategory, taskMethod)],
                (_, array) => array.Add((taskCategory, taskMethod)));
        }

        INamedTypeSymbol? resultTypeSymbol = null;
        if (symbol.ReturnType.IsValidTaskReturnType(context.Compilation) &&
            symbol.ReturnType is INamedTypeSymbol returnTypeSymbol)
        {
            if (returnTypeSymbol is { IsGenericType: true })
            {
                resultTypeSymbol = returnTypeSymbol.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
            }
            else
            {
                resultTypeSymbol = returnTypeSymbol;
            }
        }
    }

    private static void CheckRequiredProperties(
        SymbolAnalysisContext context,
        INamedTypeSymbol symbol,
        TaskMethod taskMethod,
        TaskCategory category)
    {
        var properties = symbol.GetMembers().OfType<IPropertySymbol>().ToList();
        bool HasProperty(string name) => properties.Any(p => p.Name == name);

        switch (symbol.Name)
        {
            case "Input" when category == TaskCategory.Converter:
                {
                    var types = taskMethod.GetConverterTypes();
                    if (types.HasValue && !HasProperty(types.Value.From))
                        context.ReportDiagnostic(Diagnostic.Create(TypeRules.RequiredPropertyMissing,
                            symbol.Locations.FirstOrDefault(), types.Value.From));

                    break;
                }
            case "Options":
                {
                    if (!HasProperty("ThrowErrorOnFailure"))
                        context.ReportDiagnostic(Diagnostic.Create(TypeRules.RequiredPropertyMissing,
                            symbol.Locations.FirstOrDefault(), symbol.Name, "ThrowErrorOnFailure"));
                    else
                        CheckDefaultValue(context, properties.First(p => p.Name == "ThrowErrorOnFailure"));

                    if (!HasProperty("ErrorMessageOnFailure"))
                        context.ReportDiagnostic(Diagnostic.Create(TypeRules.RequiredPropertyMissing,
                            symbol.Locations.FirstOrDefault(), symbol.Name, "ErrorMessageOnFailure"));
                    else
                        CheckDefaultValue(
                            context, properties.First(p => p.Name == "ErrorMessageOnFailure"));

                    break;
                }
        }
    }

    private static void CheckDefaultValue(
        SymbolAnalysisContext context,
        IPropertySymbol property)
    {
        var tuple = property.GetDefaultValue(context.Compilation, context.CancellationToken);
        if (tuple is null) return;
        var (value, location) = tuple.Value;


        switch (property.Name)
        {
            case "ThrowErrorOnFailure":
                if (value is true) break;
                context.ReportDiagnostic(Diagnostic.Create(TypeRules.IncorrectPropertyDefaultValue, location,
                    property.Name, "true"));
                break;
            case "ErrorMessageOnFailure":
                if (value is "") break;
                context.ReportDiagnostic(Diagnostic.Create(TypeRules.IncorrectPropertyDefaultValue, location,
                    property.Name, "\"\""));
                break;
        }
    }

    private sealed class NamedTypeSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol>
    {
        public static NamedTypeSymbolEqualityComparer Instance => new();

        public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y) =>
            SymbolEqualityComparer.Default.Equals(x, y);

        public int GetHashCode(INamedTypeSymbol obj) =>
            SymbolEqualityComparer.Default.GetHashCode(obj);
    }
}
