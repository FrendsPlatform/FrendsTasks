using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Extensions;

public static class MethodSymbolExtensions
{
    private static readonly SymbolDisplayFormat ReferenceFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    public static string ToReferenceString(this IMethodSymbol symbol) => symbol.ToDisplayString(ReferenceFormat);
}
