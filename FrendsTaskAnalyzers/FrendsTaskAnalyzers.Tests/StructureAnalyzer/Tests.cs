using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.StructureAnalyzer;
public class Tests
{
    [Theory, MemberData(nameof(TestCases.Data), MemberType = typeof(TestCases))]
    public async Task ShouldReportExpectedDiagnostics(TestCase testCase)
    {
        var test = new CSharpAnalyzerTest<Analyzers.StructureAnalyzer.StructureAnalyzer, DefaultVerifier>
        {
            TestCode = testCase.Code,
            TestState =
            {
                AdditionalFiles = { (Helpers.TaskMetadataFileName, testCase.MetadataJson) },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                AdditionalReferences =
            {
                MetadataReference.CreateFromFile(typeof(CategoryAttribute).Assembly.Location)
            }
            },
            CompilerDiagnostics = CompilerDiagnostics.None,

        };

        test.ExpectedDiagnostics.AddRange(testCase.ExpectedDiagnostics);

        if (testCase.Code.Contains("[Category("))
        {
            test.TestState.AdditionalReferences.Add(
                MetadataReference.CreateFromFile(typeof(CategoryAttribute).Assembly.Location));
        }

        await test.RunAsync();
    }
}
