using FrendsTaskAnalyzers.Analyzers.ParametersAnalyzer;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.ParameterAnalyzer;

public static class TestCases
{
    public static TheoryData<TestCase> Data()
    {
        return
        [
            // Case 1: Correct parameters should not produce diagnostics
            new TestCase
            {
                Aid = 1,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
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
                        public class Options;
                        public void Execute ([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics = []
            },

            // Case 2: Missing required parameters
            new TestCase
            {
                Aid = 2,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
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
                        public void {|#0:Execute|} ([PropertyTab] Input input, [PropertyTab] Connection connection)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(ParametersRules.RequiredParameter).WithLocation(0).WithArguments("Options"),
                    new DiagnosticResult(ParametersRules.RequiredParameter).WithLocation(0)
                        .WithArguments("CancellationToken")
                ]
            },

            // Case 3: Recommended parameter names
            new TestCase
            {
                Aid = 3,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
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
                        public void Execute ([PropertyTab] Input {|#1:invalid1|}, [PropertyTab] Connection {|#2:invalid2|}, [PropertyTab] Options {|#3:invalid3|}, CancellationToken {|#4:invalid4|})
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(ParametersRules.ParameterName).WithLocation(1).WithArguments("input"),
                    new DiagnosticResult(ParametersRules.ParameterName).WithLocation(2).WithArguments("connection"),
                    new DiagnosticResult(ParametersRules.ParameterName).WithLocation(3).WithArguments("options"),
                    new DiagnosticResult(ParametersRules.ParameterName).WithLocation(4)
                        .WithArguments("cancellationToken")
                ]
            },

            // Case 4: Missing PropertyTab attribute
            new TestCase
            {
                Aid = 4,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
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
                        public void Execute (Input {|#1:input|}, Connection {|#2:connection|}, Options {|#3:options|}, CancellationToken cancellationToken)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(ParametersRules.ParameterPropertyTabAttribute).WithLocation(1),
                    new DiagnosticResult(ParametersRules.ParameterPropertyTabAttribute).WithLocation(2),
                    new DiagnosticResult(ParametersRules.ParameterPropertyTabAttribute).WithLocation(3)
                ]
            },

            // Case 5: Not recognized parameter
            new TestCase
            {
                Aid = 5,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
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
                        public class Options;
                        public void Execute (string {|#0:foobar0|}, [PropertyTab] Input input, [PropertyTab] Options options, string {|#1:foobar1|}, CancellationToken cancellationToken, string {|#2:foobar2|})
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(ParametersRules.ParameterUnknown).WithLocation(0).WithArguments("String"),
                    new DiagnosticResult(ParametersRules.ParameterUnknown).WithLocation(1).WithArguments("String"),
                    new DiagnosticResult(ParametersRules.ParameterUnknown).WithLocation(2).WithArguments("String")
                ]
            },

            // Case 6: Incorrect parameters order
            new TestCase
            {
                Aid = 6,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Test.Execute.Test.Execute"),
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
                        public class Options;
                        public void {|#0:Execute|} (CancellationToken cancellationToken, [PropertyTab] Options options, [PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(ParametersRules.ParametersOrder).WithLocation(0)
                ]
            }
        ];
    }
}
