using FrendsTaskAnalyzers.Analyzers.DocumentationAnalyzer;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.DocumentationAnalyzer;

public static class TestCases
{
    public static TheoryData<TestCase> Data()
    {
        return
        [
            // Case 1: Valid example
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
                    /// <summary>
                    /// dummy summary
                    /// [Documentation](https://tasks.frends.com)
                    /// </summary>
                    /// <example>
                    /// dummy example
                    /// </example>
                    public class Test
                    {
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </example>
                        public class Input;
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </example>
                        public void Execute ([PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics = []
            },
            // Case 2: Missing documentation link
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
                    /// <summary>
                    /// dummy summary
                    /// </summary>
                    /// <example>
                    /// dummy example
                    /// </example>
                    public class {|#0:Test|}
                    {
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </example>
                        public class Input;
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </example>
                        public void Execute ([PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics = [new DiagnosticResult(DocumentationRules.DocumentationLinkMissing).WithLocation(0)]
            },
            // Case 3: Unsupported tags used
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
                    /// <summary>
                    /// dummy summary
                    /// [Documentation](https://tasks.frends.com)
                    /// </summary>
                    /// <example>
                    /// dummy example
                    /// </example>
                    public class Test
                    {
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// <see cref="…"></see>
                        /// <seealso cref="…"></seealso>
                        /// </example>
                        public class {|#0:Input|};
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// <cref>code example</cref>
                        /// </example>
                        public void {|#1:Execute|} ([PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics = [
                    new DiagnosticResult(DocumentationRules.UnsupportedTagsUsed).WithLocation(0).WithArguments("see", "cref"),
                    new DiagnosticResult(DocumentationRules.UnsupportedTagsUsed).WithLocation(0).WithArguments("seealso", "cref"),
                    new DiagnosticResult(DocumentationRules.UnsupportedTagsUsed).WithLocation(1).WithArguments("cref", "<ANY>")
                ]
            },
            // Case 4: Required tags missing
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
                    /// <summary>
                    /// dummy summary
                    /// [Documentation](https://tasks.frends.com)
                    /// </summary>
                    public class {|#0:Test|}
                    {
                        /// <example>
                        /// dummy example
                        /// </example>
                        public class {|#1:Input|};
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        public void {|#2:Execute|} ([PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(DocumentationRules.RequiredTagsMissing).WithLocation(0).WithArguments("example"),
                    new DiagnosticResult(DocumentationRules.RequiredTagsMissing).WithLocation(1).WithArguments("summary"),
                    new DiagnosticResult(DocumentationRules.RequiredTagsMissing).WithLocation(2).WithArguments("example")
                ]
            },
            // Case 5: Invalid documentation (malformed XML)
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
                    /// <summary>
                    /// dummy summary
                    /// [Documentation](https://tasks.frends.com)
                    /// </summary>
                    /// <example>
                    /// dummy example
                    /// </example>
                    public class Test
                    {
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </example>
                        public class Input;
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </invalidTag>
                        public void {|#0:Execute|} ([PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics = [new DiagnosticResult(DocumentationRules.DocumentationInvalid).WithLocation(0)]
            },
            // Case 6: Missing XML documentation
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
                    /// <summary>
                    /// dummy summary
                    /// [Documentation](https://tasks.frends.com)
                    /// </summary>
                    /// <example>
                    /// dummy example
                    /// </example>
                    public class Test
                    {
                        public class {|#0:Input|};
                        /// <summary>
                        /// dummy summary
                        /// </summary>
                        /// <example>
                        /// dummy example
                        /// </example>
                        public void Execute ([PropertyTab] Input input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(DocumentationRules.RequiredTagsMissing).WithLocation(0).WithArguments("summary"),
                    new DiagnosticResult(DocumentationRules.RequiredTagsMissing).WithLocation(0).WithArguments("example")
                ]
            }
        ];
    }
}
