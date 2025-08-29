// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Frends.Definitions;

namespace Frends.Echo.Execute
{
    public class Echo
    {
        public static Task<Result> Execute(Options options)
        {
            throw new NotImplementedException();
        }
    }

    public class Options
    {
        public string Test { get; set; }
    }

    public class Destination
    {
    }

    public class Input
    {
    }
}
