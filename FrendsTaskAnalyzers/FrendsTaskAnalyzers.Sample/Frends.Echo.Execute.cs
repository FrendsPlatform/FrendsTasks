// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Echo.Execute
{
    public class Echo
    {
        [Category("HTTP")]
        public Task<Result> Execute(
            [PropertyTab] Input input,
            Options options,
            [PropertyTab] Destination destination,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class Options { }
    public class Result
    {
    }
    public class Destination { }
    public class Input { }
}
