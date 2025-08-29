using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FrendsTaskAnalyzers.Extensions;

public static class PropertySymbolExtensions
{
    public static (object?, Location)? GetDefaultValue(this IPropertySymbol symbol, Compilation compilation,
        CancellationToken cancellationToken)
    {
        var defaultValueAttributeSymbol =
            compilation.GetTypeByMetadataName("System.ComponentModel.DefaultValueAttribute");

        var attribute = symbol.GetAttributes().FirstOrDefault(a =>
            SymbolEqualityComparer.Default.Equals(a.AttributeClass, defaultValueAttributeSymbol));

        var syntax = attribute?.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
        var location = syntax?.ArgumentList?.Arguments.FirstOrDefault()?.Expression.GetLocation() ??
                       syntax?.Name.GetLocation();
        if (location is null) return null;

        return (attribute?.ConstructorArguments.FirstOrDefault().Value, location);
    }
}
