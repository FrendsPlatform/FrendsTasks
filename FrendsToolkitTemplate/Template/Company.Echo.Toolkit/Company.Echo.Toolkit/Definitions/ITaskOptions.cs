namespace Company.Echo.Toolkit.Definitions;

/// <summary>
/// Default options for a task.
/// </summary>
public interface ITaskOptions
{
    /// <summary>
    /// Throw an error if the task fails. Otherwise return a Result object with Success = false;
    /// </summary>
    bool ThrowErrorOnFailure { get; set; }

    /// <summary>
    /// Custom error message that can be put into Error.Message.
    /// </summary>
    string ErrorMessageOnFailure { get; set; }
}
