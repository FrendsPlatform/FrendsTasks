using FrendsTaskAnalyzers.Analyzers.TypeAnalyzer;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace FrendsTaskAnalyzers.Tests.TypeAnalyzer;

public static class TestCases
{
    public static TheoryData<TestCase> Data()
    {
        return
        [
            new TestCase
            {
                Aid = 1,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Threading.Tasks;
                namespace Frends.Echo.Execute;

                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }

                public class Options
                {
                    public bool ThrowErrorOnFailure { get; set; } = true;
                    public string ErrorMessageOnFailure { get; set; } = "";
                }

                public class Result { }
                """,
                ExpectedDiagnostics = []
            },

            new TestCase
            {
                Aid = 2,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Threading.Tasks;
                namespace Frends.Echo.Execute;

                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }

                public class Options
                {
                    // Missing ThrowErrorOnFailure
                    // Missing ErrorMessageOnFailure
                }

                public class Result { }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.RequiredPropertyMissing)
                        .WithArguments("Options", "ThrowErrorOnFailure")
                        .WithSpan(10, 14, 10, 21),
                    new DiagnosticResult(TypeRules.RequiredPropertyMissing)
                        .WithArguments("Options", "ErrorMessageOnFailure")
                        .WithSpan(10, 14, 10, 21)
                ]
            },

            new TestCase
            {
                Aid = 3,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Threading.Tasks;
                namespace Frends.Echo.Execute;

                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }

                public class Options
                {
                    public bool ThrowErrorOnFailure { get; set; } = false; // wrong default
                    public string ErrorMessageOnFailure { get; set; } = "oops"; // wrong default
                }

                public class Result { }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.IncorrectPropertyDefaultValue)
                        .WithArguments("ThrowErrorOnFailure", "true")
                        .WithSpan(12, 17, 12, 36),
                    new DiagnosticResult(TypeRules.IncorrectPropertyDefaultValue)
                        .WithArguments("ErrorMessageOnFailure", "\"\"")
                        .WithSpan(13, 19, 13, 40)
                ]
            },

            new TestCase
            {
                Aid = 4,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.ComponentModel;
                using System.Threading.Tasks;
                namespace Frends.Echo.Execute;
                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }
                public class Options
                {
                    [DefaultValue(false)]
                    public bool ThrowErrorOnFailure { get; set; }
        
                    [DefaultValue("oops")]
                    public string ErrorMessageOnFailure { get; set; }
                }
                public class Result { }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.IncorrectPropertyDefaultValue)
                        .WithArguments("ThrowErrorOnFailure", "true")
                        .WithSpan(12, 17, 12, 36),
                    new DiagnosticResult(TypeRules.IncorrectPropertyDefaultValue)
                        .WithArguments("ErrorMessageOnFailure", "\"\"")
                        .WithSpan(15, 19, 15, 40)
                ]
            },

            new TestCase
            {
                Aid = 5,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Net.Http;
                using System.Threading.Tasks;

                namespace Frends.Echo.Execute;

                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }
                             
                public class Options
                {
                    public HttpClient Client { get; set; } // external type
                    public bool ThrowErrorOnFailure { get; set; } = true;
                    public string ErrorMessageOnFailure { get; set; } = "";
                }

                public class Result { }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Client")
                        .WithSpan(14, 23, 14, 29)
                ]
            },

            new TestCase
            {
                Aid = 6,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Net.Http;
                using System.Threading.Tasks;
    
                namespace Frends.Echo.Execute.Definitions
                {
                    public class Options
                    {
                        public HttpClient Client { get; set; } // external type
                        public bool ThrowErrorOnFailure { get; set; } = true;
                        public string ErrorMessageOnFailure { get; set; } = "";
                    }
                }
    
                namespace Frends.Echo.Execute
                {
                    public class Echo
                    {
                        public static Task<Result> Execute(Definitions.Options options) => throw new NotImplementedException();
                    }
        
                    public class Result { }
                }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Client")
                        .WithSpan(9, 27, 9, 33)
                ]
            },

            new TestCase
            {
                Aid = 7,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Threading.Tasks;
                using Newtonsoft.Json.Linq;
    
                namespace Frends.Echo.Execute;
    
                public class CustomType
                {
                    public string Value { get; set; }
                }
    
                public class Options
                {
                    public CustomType Custom { get; set; } // same namespace - OK
                    public string Message { get; set; } // primitive - OK
                    public bool ThrowErrorOnFailure { get; set; } = true;
                    public string ErrorMessageOnFailure { get; set; } = "";
                }
    
                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }
    
                public class Result { }
                """,
                ExpectedDiagnostics = []
            },

            new TestCase
            {   
                Aid = 8,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Text;
                using System.Threading.Tasks;
    
                namespace Frends.Echo.Execute.Definitions
                {
                    public class Options
                    {
                        public bool ThrowErrorOnFailure { get; set; } = true;
                        public string ErrorMessageOnFailure { get; set; } = "";
                    }
        
                    public class Result
                    {
                        public StringBuilder Builder { get; set; } // external type in Result
                    }
                }
    
                namespace Frends.Echo.Execute
                {
                    using Frends.Echo.Execute.Definitions;
        
                    public class Echo
                    {
                        public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                    }
                }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Builder")
                        .WithSpan(15, 30, 15, 37)
                ]
            },

            new TestCase
            {
                Aid = 9,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Net.Http;
                using System.Net;
                using System.Threading.Tasks;
    
                namespace Frends.Echo.Execute;
    
                public class Options
                {
                    public HttpClient Client { get; set; } // external type
                    public WebClient WebClient { get; set; } // another external type
                    public bool ThrowErrorOnFailure { get; set; } = true;
                    public string ErrorMessageOnFailure { get; set; } = "";
                }
    
                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }
    
                public class Result { }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Client")
                        .WithSpan(10, 23, 10, 29),
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("WebClient")
                        .WithSpan(11, 22, 11, 31)
                ]
            },

            new TestCase
            {
                Aid = 10,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Threading.Tasks;
    
                namespace System.Net.Http
                {
                    public class HttpClient { }
                    public class HttpResponseMessage { }
                }
    
                namespace Frends.Echo.Execute;
    
                public class Options
                {
                    public System.Net.Http.HttpClient Client { get; set; } // external type
                    public bool ThrowErrorOnFailure { get; set; } = true;
                    public string ErrorMessageOnFailure { get; set; } = "";
                }
    
                public class Result
                {
                    public System.Net.Http.HttpResponseMessage Response { get; set; } // external type in Result
                }
    
                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Client")
                        .WithSpan(14, 39, 14, 45),
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Response")
                        .WithSpan(21, 48, 21, 56)
                ]
            },

            new TestCase
            {
                Aid = 11,
                MetadataJson = Helpers.CreateMetadataJson("Frends.Echo.Execute.Echo.Execute"),
                Code =
                """
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
    
                namespace System.Net.Http
                {
                    public class HttpClient { }
                    public class HttpResponseMessage { }
                }
    
                namespace Frends.Echo.Execute;
    
                public class Options
                {
                    public List<System.Net.Http.HttpClient> Clients { get; set; } // generic
                    public System.Net.Http.HttpClient[] ClientArray { get; set; } // array
                    public System.Net.Http.HttpClient? NullableClient { get; set; } // nullable
                    public bool ThrowErrorOnFailure { get; set; } = true;
                    public string ErrorMessageOnFailure { get; set; } = "";
                }
    
                public class Result
                {
                    public Dictionary<string, System.Net.Http.HttpResponseMessage> Responses { get; set; } // generic dictionary
                }
    
                public class Echo
                {
                    public static Task<Result> Execute(Options options) => throw new NotImplementedException();
                }
                """,
                ExpectedDiagnostics =
                [
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Clients")
                        .WithSpan(15, 45, 15, 52),
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("ClientArray")
                        .WithSpan(16, 41, 16, 52),
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("NullableClient")
                        .WithSpan(17, 40, 17, 54),
                    new DiagnosticResult(TypeRules.ExposedThirdPartyType)
                        .WithArguments("Responses")
                        .WithSpan(24, 68, 24, 77)
                ]
            },
        ];
    }
}

