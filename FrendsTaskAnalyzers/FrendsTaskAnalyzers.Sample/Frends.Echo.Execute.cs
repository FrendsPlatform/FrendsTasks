// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Echo.Execute;

/// <summary>
/// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Echo.Execute)
/// </summary>
/// <example>bar</example>
public static class Echo
{
    /// <summary>/// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Echo.Execute)</summary>
    /// <example>bar</example>
    [Category("HTTP")]
    public static Task<Result> Execute(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new Result());
    }
}

/// <summary>foo</summary>
/// <example>bar</example>
public class Options
{
    /// <summary>foo</summary>
    /// <example>bar</example>
    protected bool ThrowErrorOnFailure { get; set; }
    /// <summary>foo</summary>
    /// <example>bar</example>
    protected string? ErrorMessageOnFailure { get; set; }
}

/// <summary>foo</summary>
/// <example>bar</example>
public class Result
{
    protected bool Success { get; set; }
    protected object? Error { get; set; } = null;
    protected object? Body { get; set; } = null;
    protected int StatusCode { get; set; }
    /// <summary>foo</summary>
    /// <example>bar</example>
    protected DateTime? Date { get; set; }
}

/// <summary>foo</summary>
/// <example>bar</example>
public class Destination;

/// <summary>foo</summary>
/// <example>bar</example>
public class Input;
