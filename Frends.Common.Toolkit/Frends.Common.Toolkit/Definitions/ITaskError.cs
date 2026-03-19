using System;

namespace Frends.Common.Toolkit.Definitions;

public interface ITaskError
{
    string Message { get; set; }

    Exception AdditionalInfo { get; set; }
}
