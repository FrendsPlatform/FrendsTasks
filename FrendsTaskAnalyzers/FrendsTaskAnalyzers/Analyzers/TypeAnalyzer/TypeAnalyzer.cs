using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        if (!AssignTaskMethods(context))
            return;

        var types = FindTypesForAnalysis(context.Compilation, TaskMethods);

        context.RegisterSymbolAction(ctx => AnalyzeNamedTypes(ctx, types), SymbolKind.NamedType);
    }

    private static ImmutableDictionary<INamedTypeSymbol, ImmutableArray<(TaskMethod TaskMethod, TaskCategory Category)>>
        FindTypesForAnalysis(Compilation compilation, IImmutableList<TaskMethod> taskMethods)
    {
        var allNamedTypes = GetAllNamedTypesFromSource(compilation).ToList();

        var allMethodSymbols = allNamedTypes
            .SelectMany(t => t.GetMembers().OfType<IMethodSymbol>())
            .ToList();

        var query = allMethodSymbols
            .SelectMany(m =>
            {
                var taskMethod = taskMethods.FirstOrDefault(t =>
                    m.ToReferenceString().StartsWith(t.Path, StringComparison.Ordinal));

                if (taskMethod is null)
                    return [];

                var taskCategory = m.GetTaskCategory(compilation);

                var types = m.Parameters.Select(p => p.Type).OfType<INamedTypeSymbol>().ToImmutableArray();

                // Handle Task<T>
                if (m.ReturnType is INamedTypeSymbol returnTypeSymbol)
                {
                    types = types.Add(returnTypeSymbol);

                    if (returnTypeSymbol.IsGenericType &&
                        returnTypeSymbol.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
                    {
                        if (returnTypeSymbol.TypeArguments.FirstOrDefault() is INamedTypeSymbol innerType)
                        {
                            types = types.Add(innerType);
                        }
                    }
                }

                return types.Select(t => (Type: t, TaskMethod: taskMethod, Category: taskCategory));
            })
            .GroupBy(x => x.Type, NamedTypeSymbolEqualityComparer.Instance)
            .ToImmutableDictionary(
                g => g.Key,
                g => g.Select(x => (x.TaskMethod, x.Category)).ToImmutableArray(),
                NamedTypeSymbolEqualityComparer.Instance);

        return query;
    }

    private static void AnalyzeNamedTypes(
        SymbolAnalysisContext context,
        ImmutableDictionary<INamedTypeSymbol, ImmutableArray<(TaskMethod TaskMethod, TaskCategory Category)>> types)
    {
        if (context.Symbol is not INamedTypeSymbol symbol) return;
        if (!types.TryGetValue(symbol, out var array)) return;

        foreach (var (taskMethod, category) in array)
        {
            CheckRequiredProperties(context, symbol, taskMethod, category);
            CheckExposedThirdPartyTypes(context, symbol, taskMethod);
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

        if (symbol.Name == "Options")
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
        }
    }

    private static void CheckDefaultValue(SymbolAnalysisContext context, IPropertySymbol property)
    {
        var tuple = property.GetDefaultValue(context.Compilation, context.CancellationToken);
        if (tuple is null) return;
        var (value, _) = tuple.Value;

        var diagnosticLocation = property.DeclaringSyntaxReferences
            .FirstOrDefault()
            ?.GetSyntax(context.CancellationToken) is PropertyDeclarationSyntax propSyntax
            ? propSyntax.Identifier.GetLocation()
            : property.Locations.FirstOrDefault();

        switch (property.Name)
        {
            case "ThrowErrorOnFailure":
                if (value is true) break;
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeRules.IncorrectPropertyDefaultValue,
                    diagnosticLocation,
                    property.Name,
                    "true"));
                break;

            case "ErrorMessageOnFailure":
                if (value is "") break;
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeRules.IncorrectPropertyDefaultValue,
                    diagnosticLocation,
                    property.Name,
                    "\"\""));
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

    private static void CheckExposedThirdPartyTypes(
    SymbolAnalysisContext context,
    INamedTypeSymbol symbol,
    TaskMethod taskMethod)
    {
        var taskNamespace = GetNamespacePrefix(taskMethod.Path);

        if (!IsAllowedType(symbol, taskNamespace))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                TypeRules.ExposedThirdPartyType,
                symbol.Locations.FirstOrDefault(),
                symbol.Name));
        }

        foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (!IsPropertyTypeAllowed(property.Type, taskNamespace))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeRules.ExposedThirdPartyType,
                    property.Locations.FirstOrDefault(),
                    property.Name));
            }
        }
    }

    private static bool IsAllowedType(INamedTypeSymbol type, string taskNamespace)
    {
        if (type.ContainingNamespace == null)
            return true;

        var ns = type.ContainingNamespace.ToDisplayString();

        // Allowed: in same namespace prefix as task method
        if (ns.StartsWith(taskNamespace, StringComparison.Ordinal))
            return true;

        // Allowed: primitives, string, object, arrays
        if (type.SpecialType != SpecialType.None)
            return true;

        // Allowed: Task / Task<T>
        if (ns == "System.Threading.Tasks" && type.Name == "Task")
            return true;

        // Allowed: JToken adn JObject (explicit whitelist)
        if (ns == "Newtonsoft.Json.Linq" && (type.Name == "JToken" || type.Name == "JObject"))
            return true;

        return false;
    }

    private static bool IsPropertyTypeAllowed(ITypeSymbol type, string taskNamespace)
    {
        switch (type)
        {
            case IArrayTypeSymbol arrayType:
                return IsPropertyTypeAllowed(arrayType.ElementType, taskNamespace);

            case INamedTypeSymbol namedType:
                {
                    var ns = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
                    var isCollection = namedType.IsGenericType &&
                                       (ns.StartsWith("System.Collections", StringComparison.Ordinal) ||
                                        ns.StartsWith("System.Collections.Immutable", StringComparison.Ordinal));

                    if (!isCollection && !IsAllowedType(namedType, taskNamespace))
                        return false;

                    if (namedType.IsGenericType)
                    {
                        foreach (var typeArg in namedType.TypeArguments)
                            if (!IsPropertyTypeAllowed(typeArg, taskNamespace))
                                return false;
                    }
                    return true;
                }
            default:
                return true;
        }
    }

    private static string GetNamespacePrefix(string taskPath)
    {
        var parts = taskPath.Split('.');
        if (parts.Length > 2)
            return string.Join(".", parts.Take(parts.Length - 2));
        return taskPath;
    }

#pragma warning disable RS1030
    private static IEnumerable<INamedTypeSymbol> GetAllNamedTypesFromSource(Compilation compilation)
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            foreach (var node in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(node) as INamedTypeSymbol;
                if (symbol != null)
                    yield return symbol;
            }
        }
    }
}
