using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests;

public class TaskMethodAnalyzerTests
{
    [Fact]
    public async Task ShouldReportDiagnostic_WhenCancellationTokenParameterIsMissing()
    {
        // language=editorconfig
        const string editorconfig =
            """
            root = true
            [*.cs]
            frends_task_analyzers.task_methods=Frends.Echo.Execute.Echo.Execute
            """;

        // language=csharp
        const string code =
            """
            using System;
            using System.ComponentModel;

            namespace Frends.Echo.Execute;

            public class Echo
            {
                public Result {|#0:Execute|}([PropertyTab] Input input)
                {
                    throw new NotImplementedException();
                }
            }

            public class Result;
            public class Input;
            """;

        await new CSharpAnalyzerTest<TaskMethodAnalyzer, DefaultVerifier>
        {
            TestCode = code,
            TestState =
            {
                AnalyzerConfigFiles = { ("/.editorconfig", editorconfig) },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(Rules.TaskMethodRequiredParameter)
                        .WithLocation(0).WithArguments("CancellationToken")
                }
            }
        }.RunAsync();
    }
}
