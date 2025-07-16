using FrendsTaskAnalyzers.BaseAnalyzer;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.BaseAnalyzer;

public static class TestCases
{
    public static TheoryData<TestCase> Data()
    {
        return
        [
            // Case 1: Missing metadata
            new TestCase
            {
                Aid = 1,
                MetadataJson = Helpers.CreateMetadataJson(""),
                Code =
                    // language=csharp
                    """
                    using System;
                    using System.ComponentModel;
                    using System.Threading;
                    namespace Frends.Test.Execute;
                    public class Test
                    {
                        public class Input;
                        public class Connection;
                        public class Options;
                        public void Execute ([PropertyTab] Input input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(BaseRules.ConfigurationMissing),
                    new DiagnosticResult(BaseRules.ConfigurationMissing),
                    new DiagnosticResult(BaseRules.ConfigurationMissing),
                    new DiagnosticResult(BaseRules.ConfigurationMissing),
                ]
            }
        ];
    }
}
