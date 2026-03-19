namespace Frends.Common.Toolkit.Definitions;

public interface ITaskResult<TError> where TError : ITaskError
{
    bool Success { get; set; }
    TError Error { get; set; }
}
