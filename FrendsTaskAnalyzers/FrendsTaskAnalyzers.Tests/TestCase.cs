using Microsoft.CodeAnalysis.Testing;

namespace FrendsTaskAnalyzers.Tests;

public class TestCase
{
    // Unique identifier for the test case to simplify find a case while debugging
    public required int Aid { get; init; }
    public required string MetadataJson { get; init; }
    public required string Code { get; init; }
    public required DiagnosticResult[] ExpectedDiagnostics { get; init; }
}
