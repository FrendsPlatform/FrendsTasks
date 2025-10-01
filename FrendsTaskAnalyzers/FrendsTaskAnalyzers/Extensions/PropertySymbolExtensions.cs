using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FrendsTaskAnalyzers.Extensions;

public static class PropertySymbolExtensions
{
    public static (object? Value, Location Location)? GetDefaultValue(
        this IPropertySymbol symbol,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is PropertyDeclarationSyntax propDecl)
        {
            if (propDecl.Initializer?.Value is ExpressionSyntax expr)
            {
                var model = compilation.GetSemanticModel(propDecl.SyntaxTree);
                var constValue = model.GetConstantValue(expr, cancellationToken);
                if (constValue.HasValue)
                {
                    return (constValue.Value, expr.GetLocation());
                }
            }
        }

        var defaultValueAttr = compilation.GetTypeByMetadataName("System.ComponentModel.DefaultValueAttribute");
        var attribute = symbol.GetAttributes().FirstOrDefault(a =>
            SymbolEqualityComparer.Default.Equals(a.AttributeClass, defaultValueAttr));

        if (attribute is not null)
        {
            var location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken)?.GetLocation();
            var value = attribute.ConstructorArguments.FirstOrDefault().Value;
            if (location is not null)
                return (value, location);
        }

        return null;
    }
}
