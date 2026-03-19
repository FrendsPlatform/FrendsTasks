using System;
using Frends.Common.Toolkit.Definitions;

namespace Frends.Common.Toolkit.Handlers;

public static class ErrorHandler
{
    public static ITaskResult<TError> Handle<TError, TResult>(Exception exception, bool throwOnFailure,
        string errorMessageOnFailure)
        where TResult : ITaskResult<TError>, new()
        where TError : ITaskError, new()
    {
        if (throwOnFailure)
        {
            if (string.IsNullOrEmpty(errorMessageOnFailure))
                throw new Exception(exception.Message, exception);

            throw new Exception(errorMessageOnFailure, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(errorMessageOnFailure)
            ? $"{errorMessageOnFailure}: {exception.Message}"
            : exception.Message;

        return new TResult
        {
            Success = false,
            Error = new TError
            {
                Message = errorMessage,
                AdditionalInfo = exception,
            },
        };
    }
}
