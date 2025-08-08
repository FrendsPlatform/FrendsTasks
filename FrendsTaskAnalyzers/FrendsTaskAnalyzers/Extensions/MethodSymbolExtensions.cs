using System.Linq;
using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Extensions;

public static class MethodSymbolExtensions
{
    private static readonly SymbolDisplayFormat ReferenceFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    public static string ToReferenceString(this IMethodSymbol symbol) => symbol.ToDisplayString(ReferenceFormat);

    public static string? GetCategory(this IMethodSymbol methodSymbol)
    {
        var categoryAttribute = methodSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "CategoryAttribute");

        return categoryAttribute?.ConstructorArguments.FirstOrDefault().Value as string;
    }
}
