using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FrendsTaskAnalyzers.Tests.BaseAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
// This analyzer is used only to test the base functionality of the BaseAnalyzer class.
#pragma warning disable RS1036
public class TestBaseAnalyzer : FrendsTaskAnalyzers.BaseAnalyzer.BaseAnalyzer
#pragma warning restore RS1036
{
    protected override ImmutableArray<DiagnosticDescriptor> AdditionalDiagnostics { get; } = [];
}
