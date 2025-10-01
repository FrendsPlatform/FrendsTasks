// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

using System;
using System.Threading.Tasks;
using Frends.Echo.Execute.Definitions;

namespace Frends.Echo.Execute
{
    public class Echo
    {
        public static Task<Result> Execute(Options options, Input input)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Frends.Echo.Execute.Definitions
{
    using System.Net.Http;
    using System.ComponentModel;
    using Newtonsoft.Json.Linq;

    public class Options
    {
        public string? Test { get; set; }

        [DefaultValue(false)]
        public bool ThrowErrorOnFailure { get; set; }

        public HttpClient Client { get; set; }

        public JObject JsonData { get; set; }
    }

    public class Input
    {
        public string Message { get; set; }
    }

    public class Result
    {
        public string Output { get; set; }

        public JToken Token { get; set; }
    }
}
