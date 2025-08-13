using System.Linq;
using FrendsTaskAnalyzers.Models;
using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Extensions;

public static class MethodSymbolExtensions
{
    private static readonly SymbolDisplayFormat ReferenceFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    public static string ToReferenceString(this IMethodSymbol symbol) => symbol.ToDisplayString(ReferenceFormat);

    public static TaskCategory GetCategory(this IMethodSymbol symbol, Compilation compilation)
    {
        var categoryAttributeSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.CategoryAttribute");
        if (categoryAttributeSymbol is null) return TaskCategory.Generic;

        var attribute = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Equals(categoryAttributeSymbol, SymbolEqualityComparer.Default) == true);

        var category = (attribute?.ConstructorArguments.FirstOrDefault().Value as string)?.ToLowerInvariant();

        return category switch
        {
            "converter" => TaskCategory.Converter,
            "database" => TaskCategory.Database,
            "file" => TaskCategory.File,
            "http" => TaskCategory.Http,
            _ => TaskCategory.Generic,
        };
    }
}
