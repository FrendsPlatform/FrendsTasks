using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FrendsTaskAnalyzers.Extensions;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Analyzers.StructureAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StructureAnalyzer : BaseAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        StructureRules.ClassShouldBeStatic,
        StructureRules.MethodShouldBeStatic,
        StructureRules.MethodOverloadNotAllowed,
        StructureRules.ReturnTypeIncorrect,
        StructureRules.ReturnTypeMissingProperties
    ];

    protected override void RegisterActions(CompilationStartAnalysisContext context)
    {
        var reportedDiagnostics = new HashSet<(ISymbol Symbol, string DiagnosticId)>();
        var reportedMissingProperties = new HashSet<(ISymbol Symbol, string DiagnosticId, string PropertyName)>();

        context.RegisterSymbolAction(ctx =>
        {
            if (TaskMethods is null) return;
            AnalyzeMethod(ctx, TaskMethods, reportedDiagnostics, reportedMissingProperties);
        }, SymbolKind.Method);

        context.RegisterSymbolAction(ctx =>
        {
            if (TaskMethods is null) return;
            AnalyzeClass(ctx, TaskMethods, reportedDiagnostics);
        }, SymbolKind.NamedType);
    }

    private static void AnalyzeClass(
        SymbolAnalysisContext context,
        IImmutableList<TaskMethod> taskMethods,
        HashSet<(ISymbol Symbol, string DiagnosticId)> reportedDiagnostics)
    {
        if (context.Symbol is not INamedTypeSymbol classSymbol) return;

        var anyTaskMethod = taskMethods.Any(t => t.Path.Contains(classSymbol.Name));
        if (!anyTaskMethod) return;

        if (!classSymbol.IsStatic)
            ReportOnce(context, StructureRules.ClassShouldBeStatic, classSymbol, reportedDiagnostics,
                classSymbol.Name);
    }

    private static void AnalyzeMethod(
        SymbolAnalysisContext context,
        IImmutableList<TaskMethod> taskMethods,
        HashSet<(ISymbol Symbol, string DiagnosticId)> reportedDiagnostics,
        HashSet<(ISymbol Symbol, string DiagnosticId, string PropertyName)> reportedMissingProperties)
    {
        if (context.Symbol is not IMethodSymbol methodSymbol) return;

        var taskMethod = taskMethods.FirstOrDefault(t => t.Path == methodSymbol.ToReferenceString());
        if (taskMethod is null) return;

        var containingClass = methodSymbol.ContainingType;

        // 1. Class static
        // if (!containingClass.IsStatic)
        //     ReportOnce(context, StructureRules.ClassShouldBeStatic, containingClass, reportedDiagnostics,
        //         containingClass.Name);

        // 2. Method static
        if (!methodSymbol.IsStatic)
            ReportOnce(context, StructureRules.MethodShouldBeStatic, methodSymbol, reportedDiagnostics,
                methodSymbol.Name);

        // 3. Method overloads
        var overloads = containingClass
            .GetMembers(methodSymbol.Name)
            .OfType<IMethodSymbol>()
            .ToList();

        if (overloads.Count > 1 && SymbolEqualityComparer.Default.Equals(overloads[0], methodSymbol))
            ReportOnce(context, StructureRules.MethodOverloadNotAllowed, methodSymbol, reportedDiagnostics,
                methodSymbol.Name);

        // 4. Return type
        var returnType = methodSymbol.ReturnType;
        if (!returnType.IsValidTaskReturnType(context.Compilation))
            ReportOnce(context, StructureRules.ReturnTypeIncorrect, methodSymbol, reportedDiagnostics,
                "Result or Task<Result>");
        else
        {
            var actualReturnType = returnType.Name == "Result"
                ? returnType
                : ((INamedTypeSymbol)returnType).TypeArguments.First();

            var category = methodSymbol.GetTaskCategory(context.Compilation);
            CheckRequiredProperties(context, methodSymbol, actualReturnType, category, reportedMissingProperties);
        }
    }

    private static void ReportOnce(
        SymbolAnalysisContext context,
        DiagnosticDescriptor rule,
        ISymbol symbol,
        HashSet<(ISymbol Symbol, string DiagnosticId)> reportedDiagnostics,
        params object[] messageArgs)
    {
        var key = (symbol, rule.Id);
        if (!reportedDiagnostics.Add(key)) return;

        var location = Location.None;

        if (symbol.DeclaringSyntaxReferences.FirstOrDefault() is { } syntaxRef)
        {
            var syntax = syntaxRef.GetSyntax(context.CancellationToken);
            location = syntax switch
            {
                ClassDeclarationSyntax cls => cls.Identifier.GetLocation(),
                MethodDeclarationSyntax m => m.Identifier.GetLocation(),
                _ => syntax.GetLocation()
            };
        }

        context.ReportDiagnostic(Diagnostic.Create(rule, location, messageArgs));
    }

    private static void CheckRequiredProperties(
        SymbolAnalysisContext context,
        IMethodSymbol methodSymbol,
        ITypeSymbol returnType,
        TaskCategory category,
        HashSet<(ISymbol Symbol, string DiagnosticId, string PropertyName)> reportedMissingProperties)
    {
        var properties = returnType.GetMembers().OfType<IPropertySymbol>().ToList();
        bool HasProperty(string name) => properties.Any(p => p.Name == name);

        if (!HasProperty("Success"))
            ReportMissingProperty(context, methodSymbol, "Success", reportedMissingProperties);

        if (!HasProperty("Error"))
            ReportMissingProperty(context, methodSymbol, "Error", reportedMissingProperties);

        switch (category)
        {
            case TaskCategory.Database when !HasProperty("Data"):
                ReportMissingProperty(context, methodSymbol, "Data", reportedMissingProperties);
                break;
            case TaskCategory.Http:
                {
                    if (!HasProperty("Body"))
                        ReportMissingProperty(context, methodSymbol, "Body", reportedMissingProperties);
                    if (!HasProperty("StatusCode"))
                        ReportMissingProperty(context, methodSymbol, "StatusCode",
                            reportedMissingProperties);
                    break;
                }
            case TaskCategory.Converter:
                {
                    // TODO: This is supposed to be name from the task name if possible:
                    //  e.g. "Json" for "ConvertXmlToJson"
                    if (!HasProperty("TargetFormat"))
                        ReportMissingProperty(context, methodSymbol, "TargetFormat",
                            reportedMissingProperties);
                    break;
                }
            case TaskCategory.File when !HasProperty("FilePath"):
                ReportMissingProperty(context, methodSymbol, "FilePath", reportedMissingProperties);
                break;
        }
    }

    private static void ReportMissingProperty(
        SymbolAnalysisContext context,
        IMethodSymbol methodSymbol,
        string propertyName,
        HashSet<(ISymbol Symbol, string DiagnosticId, string PropertyName)> reportedMissingProperties)
    {
        var key = (methodSymbol, StructureRules.ReturnTypeMissingProperties.Id, propertyName);
        if (!reportedMissingProperties.Add(key))
            return;

        var location = Location.None;

        if (methodSymbol.DeclaringSyntaxReferences.FirstOrDefault() is { } methodSyntaxRef)
        {
            var methodSyntax = methodSyntaxRef.GetSyntax(context.CancellationToken);
            if (methodSyntax is MethodDeclarationSyntax methodDecl)
            {
                location = methodDecl.ReturnType.GetLocation();
            }
            else
            {
                location = methodSyntax.GetLocation();
            }
        }

        if (location == Location.None)
            location = methodSymbol.Locations.FirstOrDefault() ?? Location.None;

        if (location != Location.None)
            context.ReportDiagnostic(Diagnostic.Create(StructureRules.ReturnTypeMissingProperties, location,
                propertyName));
    }
}
