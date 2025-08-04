using System.Threading.Tasks;
using FrendsTaskAnalyzers.Analyzers.ParametersAnalyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.ParameterAnalyzer;

public class Tests
{
    [Theory, MemberData(nameof(TestCases.Data), MemberType = typeof(TestCases))]
    public async Task ShouldReportExpectedDiagnostics(TestCase testCase)
    {
        var analyzerTest = new CSharpAnalyzerTest<ParametersAnalyzer, DefaultVerifier>
        {
            TestCode = testCase.Code,
            TestState = { AdditionalFiles = { (Helpers.TaskMetadataFileName, testCase.MetadataJson) } }
        };
        analyzerTest.ExpectedDiagnostics.AddRange(testCase.ExpectedDiagnostics);
        await analyzerTest.RunAsync();
    }
}
