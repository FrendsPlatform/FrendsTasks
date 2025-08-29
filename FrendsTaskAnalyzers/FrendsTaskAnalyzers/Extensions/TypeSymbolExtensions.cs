using System.Linq;
using Microsoft.CodeAnalysis;

namespace FrendsTaskAnalyzers.Extensions;

public static class TypeSymbolExtensions
{
    public static bool IsValidTaskReturnType(this ITypeSymbol symbol, Compilation compilation)
    {
        if (symbol.Name == "Result") return true;

        var genericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

        return symbol is INamedTypeSymbol namedTypeSymbol &&
               SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, genericTaskSymbol) &&
               namedTypeSymbol.TypeArguments.FirstOrDefault()?.Name == "Result";
    }
}
