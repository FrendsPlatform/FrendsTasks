using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests;

public class NameAnalyzerTests
{
    private const string TaskMetadataFileName = "FrendsTaskMetadata.json";

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task ShouldReportExpectedDiagnostics(
        string metadata, string code, IEnumerable<DiagnosticResult> expected)
    {
        var analyzerTest = new CSharpAnalyzerTest<NameAnalyzer, DefaultVerifier>
        {
            TestCode = code,
            TestState =
            {
                AdditionalFiles = { (TaskMetadataFileName, metadata) }
            }
        };
        analyzerTest.ExpectedDiagnostics.AddRange(expected);
        await analyzerTest.RunAsync();
    }

    public static IEnumerable<object[]> TestCases()
    {
        // Case 1: Correct naming should not produce diagnostics
        yield return
        [
            CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
            // language=C#
            """
            namespace Frends.Test.Execute;
            public class Test
            {
                public void Execute() {}
            }
            """,
            Array.Empty<DiagnosticResult>()
        ];

        // Case 2: Non-standard namespace should produce a diagnostic
        yield return
        [
            CreateMetadataJson("Frends.Test.Test.Execute"),
            // language=C#
            """
            namespace {|#0:Frends.Test|};
            public class Test
            {
                public void Execute() {}
            }
            """,
            new[] { new DiagnosticResult(NameAnalyzer.NamespaceRule).WithLocation(0) }
        ];

        // Case 3: Incorrect type name should produce a diagnostic
        yield return
        [
            CreateMetadataJson("Frends.TestA.Execute.TestB.Execute"),
            // language=C#
            """
            namespace Frends.TestA.Execute;
            public class {|#0:TestB|}
            {
                public void Execute() {}
            }
            """,
            new[] { new DiagnosticResult(NameAnalyzer.TypeRule).WithLocation(0).WithArguments("TestA") }
        ];

        // Case 4: Incorrect method name should produce a diagnostic
        yield return
        [
            CreateMetadataJson("Frends.Test.ExecuteA.Test.ExecuteB"),
            // language=C#
            """
            namespace Frends.Test.ExecuteA;
            public class Test
            {
                public void {|#0:ExecuteB|}() {}
            }
            """,
            new[] { new DiagnosticResult(NameAnalyzer.MethodRule).WithLocation(0).WithArguments("ExecuteA") }
        ];

        // Case 5: Incorrect type and method names should produce diagnostics
        yield return
        [
            CreateMetadataJson("Frends.TestA.ExecuteA.TestB.ExecuteB"),
            // language=C#
            """
            namespace Frends.TestA.ExecuteA;
            public class {|#0:TestB|}
            {
                public void {|#1:ExecuteB|}() {}
            }
            """,
            new[]
            {
                new DiagnosticResult(NameAnalyzer.TypeRule).WithLocation(0).WithArguments("TestA"),
                new DiagnosticResult(NameAnalyzer.MethodRule).WithLocation(1).WithArguments("ExecuteA")
            }
        ];

        // Case 6: Multiple tasks with incorrect type and method names should produce diagnostics
        yield return
        [
            CreateMetadataJson("Frends.TestA.ExecuteA.Test.Execute", "Frends.TestB.ExecuteB.Test.Execute"),
            // language=C#
            """
            namespace Frends.TestA.ExecuteA
            {
                public class {|#0:Test|}
                {
                    public void {|#1:Execute|}() {}
                }
            }
            namespace Frends.TestB.ExecuteB
            {
                public class {|#2:Test|}
                {
                    public void {|#3:Execute|}() {}
                }
            }
            """,
            new[]
            {
                new DiagnosticResult(NameAnalyzer.TypeRule).WithLocation(0).WithArguments("TestA"),
                new DiagnosticResult(NameAnalyzer.MethodRule).WithLocation(1).WithArguments("ExecuteA"),
                new DiagnosticResult(NameAnalyzer.TypeRule).WithLocation(2).WithArguments("TestB"),
                new DiagnosticResult(NameAnalyzer.MethodRule).WithLocation(3).WithArguments("ExecuteB")
            }
        ];
    }

    private static string CreateMetadataJson(params string[] taskMethods)
    {
        var tasks = taskMethods
            .Select(t => new JsonObject { ["TaskMethod"] = t })
            .ToArray<JsonNode?>();

        return new JsonObject { ["Tasks"] = new JsonArray(tasks) }.ToJsonString();
    }
}
