using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Party.Echo.Execute.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Delimiter to use between the repeated strings.
    /// </summary>
    /// <example>,</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue(" ")]
    public string Delimiter { get; set; }

    /// <summary>
    /// Whether to continue processing remaining files when a single item fails. If false, the task stops on the first failure
    /// (local filesystem tasks roll back everything processed in the current run, remote tasks keep already completed operations).
    /// If true, the failure is recorded per item and the remaining files are still processed.
    /// </summary>
    /// <example>false</example>
    // TODO: Remove this property if the task does not process batches of files (upload, download, copy, move),
    // e.g. message queue or event-driven tasks.
    [DefaultValue(false)]
    public bool ContinueOnFailure { get; set; } = false;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
