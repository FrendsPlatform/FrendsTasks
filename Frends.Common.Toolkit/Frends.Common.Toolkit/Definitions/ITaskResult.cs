namespace Frends.Common.Toolkit.Definitions;

/// <summary>
/// Result object returned by a task.
/// </summary>
/// <typeparam name="TError">Type of Error specific for a task</typeparam>
public interface ITaskResult<TError>
    where TError : ITaskError
{
    /// <summary>
    /// Flag indicating whether the task was successful.
    /// </summary>
    bool Success { get; set; }

    /// <summary>
    /// Error object containing information about the error if the task failed. Null if the task was successful.
    /// </summary>
    TError Error { get; set; }
}
