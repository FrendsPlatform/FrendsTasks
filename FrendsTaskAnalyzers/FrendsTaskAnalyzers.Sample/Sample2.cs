// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

using System;
using System.ComponentModel;
using System.Threading;

namespace Frends.Sample2.Echo;

internal class Sample2
{
    public static Result Echo(
        [PropertyTab] EchoInput input,
        Options options,
        [PropertyTab] Destination destination,
        CancellationToken token
    )
    {
        throw new NotImplementedException();
    }

    internal class EchoInput;

    internal class Destination;

    internal class Options;

    internal class Result;
}
