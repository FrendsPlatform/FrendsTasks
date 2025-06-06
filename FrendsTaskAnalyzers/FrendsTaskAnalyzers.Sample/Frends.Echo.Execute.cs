// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

using System;
using System.ComponentModel;
using System.Threading;
using Frends.Echo.Execute.Definitions;

namespace Frends.Echo.Execute
{
    public class Echo
    {
        public Result Execute(
            [PropertyTab] EchoInput input,
            Options options,
            [PropertyTab] Destination destination,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }

    namespace Definitions
    {
        public class Options;

        public class Result;

        public class Destination;

        public class EchoInput;
    }
}
