using System;

namespace Frends.Common.Toolkit.Definitions;

/// <summary>
/// Object representing an error that should be returned when task execution failed.
/// Contains a message describing the error and an optional Exception object with additional information about the error.
/// </summary>
public interface ITaskError
{
    /// <summary>
    /// Message describing the error.
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// Additional information about the error. Usually it's an Exception that was thrown
    /// </summary>
    Exception AdditionalInfo { get; set; }
}
