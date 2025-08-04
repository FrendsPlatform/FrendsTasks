using FrendsTaskAnalyzers.Analyzers.NameAnalyzer;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.NameAnalyzer;

public static class TestCases
{
    public static TheoryData<TestCase> Data()
    {
        return
        [
            // Case 1: Correct naming should not produce diagnostics
            new TestCase
            {
                Aid = 1,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                Code =
                    // language=C#
                    """
                    namespace Frends.Test.Execute;
                    public class Test
                    {
                        public void Execute() {}
                    }
                    """,
                ExpectedDiagnostics = []
            },

            // Case 2: Non-standard namespace should produce a diagnostic
            new TestCase
            {
                Aid = 2,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Test.Execute"),
                Code =
                    // language=C#
                    """
                    namespace {|#0:Frends.Test|};
                    public class Test
                    {
                        public void Execute() {}
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(NameRules.NamespaceRule).WithLocation(0)
                ]
            },

            // Case 3: Incorrect type name should produce a diagnostic
            new TestCase
            {
                Aid = 3,
                MetadataJson = Helpers.CreateMetadataJson("Frends.TestA.Execute.TestB.Execute"),
                Code =
                    // language=C#
                    """
                    namespace Frends.TestA.Execute;
                    public class {|#0:TestB|}
                    {
                        public void Execute() {}
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(NameRules.TypeRule).WithLocation(0).WithArguments("TestA")
                ]
            },

            // Case 4: Incorrect method name should produce a diagnostic
            new TestCase
            {
                Aid = 4,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.ExecuteA.Test.ExecuteB"),
                Code =
                    // language=C#
                    """
                    namespace Frends.Test.ExecuteA;
                    public class Test
                    {
                        public void {|#0:ExecuteB|}() {}
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(NameRules.MethodRule).WithLocation(0).WithArguments("ExecuteA")
                ]
            },

            // Case 5: Incorrect type and method names should produce diagnostics
            new TestCase
            {
                Aid = 5,
                MetadataJson = Helpers.CreateMetadataJson("Frends.TestA.ExecuteA.TestB.ExecuteB"),
                Code =
                    // language=C#
                    """
                    namespace Frends.TestA.ExecuteA;
                    public class {|#0:TestB|}
                    {
                        public void {|#1:ExecuteB|}() {}
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(NameRules.TypeRule).WithLocation(0).WithArguments("TestA"),
                    new DiagnosticResult(NameRules.MethodRule).WithLocation(1).WithArguments("ExecuteA")
                ]
            },

            // Case 6: Multiple tasks with incorrect type and method names should produce diagnostics
            new TestCase
            {
                Aid = 6,
                MetadataJson = Helpers.CreateMetadataJson("Frends.TestA.ExecuteA.Test.Execute",
                    "Frends.TestB.ExecuteB.Test.Execute"),
                Code =
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
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(NameRules.TypeRule).WithLocation(0).WithArguments("TestA"),
                    new DiagnosticResult(NameRules.MethodRule).WithLocation(1).WithArguments("ExecuteA"),
                    new DiagnosticResult(NameRules.TypeRule).WithLocation(2).WithArguments("TestB"),
                    new DiagnosticResult(NameRules.MethodRule).WithLocation(3).WithArguments("ExecuteB")
                ]
            }
        ];
    }
}
