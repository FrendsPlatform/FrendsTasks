using FrendsTaskAnalyzers.Analyzers.StructureAnalyzer;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.StructureAnalyzer;

public static class TestCases
{
    public static TheoryData<TestCase> Data()
    {
        return
        [
            new TestCase
            {
                Aid = 1,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public class {|#0:Test|}  // Should be static
                    {
                        public static Result Execute() => new();
                    }
                    public class Result {
                        public string Error { get; set; }
                        public bool Success { get; set; }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.ClassShouldBeStatic).WithLocation(0).WithArguments("Test")
                ]
            },

            new TestCase
            {
                Aid = 2,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public static class Test
                    {
                        public static Result Execute() => new();
                    }
                    public class Result {
                        public string Error { get; set; }
                        public bool Success { get; set; }
                    }
                    """,
                ExpectedDiagnostics = []
            },

            new TestCase
            {
                Aid = 3,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public static class Test
                    {
                        public Result {|#0:Execute|}() => new(); // Should be static
                    }
                    public class Result {
                        public string Error { get; set; }
                        public bool Success { get; set; }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.MethodShouldBeStatic)
                        .WithLocation(0).WithArguments("Execute")
                ]
            },

            new TestCase
            {
                Aid = 4,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public static class Test
                    {
                        public static Result {|#0:Execute|}() => new();
                        public static Result Execute(string input) => new();
                    }
                    public class Result {
                        public string Error { get; set; }
                        public bool Success { get; set; }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.MethodOverloadNotAllowed)
                        .WithLocation(0).WithArguments("Execute")
                ]
            },

            new TestCase
            {
                Aid = 5,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public static class Test
                    {
                        public static string {|#0:Execute|}() => "invalid"; // Should be Result
                    }
                    public class Result {
                        public string Error { get; set; }
                        public bool Success { get; set; }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.ReturnTypeIncorrect)
                        .WithLocation(0).WithArguments("Result or Task<Result>")
                ]
            },

            new TestCase
            {
                Aid = 6,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    using System.Threading.Tasks;
                    namespace Frends.Test.Execute;
                    public static class Test
                    {
                        public static Task<Result> Execute() => Task.FromResult(new Result());
                    }
                    public class Result {
                        public string Error { get; set; }
                        public bool Success { get; set; }
                    }
                    """,
                ExpectedDiagnostics = []
            },

            new TestCase
            {
                Aid = 7,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public static class Test
                    {
                        public static {|#0:Result|} Execute() => new();
                    }
                    public class Result // Missing properties
                    {
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(0).WithArguments("Error"),
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(0).WithArguments("Success")
                ]
            },

            new TestCase
            {
                Aid = 8,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Database.Query.Test.Execute"),
                // language=C#
                Code =
                    """
                    using System.ComponentModel;
                    namespace Frends.Database.Query
                    {
                        public static class Test
                        {
                            [Category("Database")]
                            public static {|#0:Result|} Execute() => new();
                        }
                        public class Result
                        {
                            public bool Success { get; set; }
                            public string Error { get; set; }
                            // Missing Data
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(0).WithArguments("Data")
                ]
            },

            new TestCase
            {
                Aid = 9,
                MetadataJson = Helpers.CreateMetadataJson("Frends.HTTP.Call.Test.Execute"),
                // language=C#
                Code =
                    """
                    using System.ComponentModel;
                    namespace Frends.HTTP.Call;
                    public static class Test
                    {
                        [Category("HTTP")]
                        public static {|#0:Result|} Execute() => new();
                    }
                    public class Result
                    {
                        public bool Success { get; set; }
                        public string Error { get; set; }
                        // Missing Body and StatusCode
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(0).WithArguments("Body"),
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(0).WithArguments("StatusCode")
                ]
            },

            new TestCase
            {
                Aid = 10,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
                // language=C#
                Code =
                    """
                    namespace Frends.Test.Execute;
                    public class {|#0:Test|} // Not static
                    {
                        public {|#3:Result|} {|#1:Execute|}() => new(); // Not static
                        public {|#4:Result|} {|#2:Execute|}(string input) => new(); // Overload
                    }
                    public class Result // Missing props
                    {
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(StructureRules.ClassShouldBeStatic)
                        .WithLocation(0).WithArguments("Test"),
                    new DiagnosticResult(StructureRules.MethodShouldBeStatic)
                        .WithLocation(1).WithArguments("Execute"),
                    new DiagnosticResult(StructureRules.MethodShouldBeStatic)
                        .WithLocation(2).WithArguments("Execute"),
                    new DiagnosticResult(StructureRules.MethodOverloadNotAllowed)
                        .WithLocation(1).WithArguments("Execute"),
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(3).WithArguments("Success"),
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(3).WithArguments("Error"),
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(4).WithArguments("Success"),
                    new DiagnosticResult(StructureRules.ReturnTypeMissingProperties)
                        .WithLocation(4).WithArguments("Error"),
                ]
            }
        ];
    }
}
