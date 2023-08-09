using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Template.Task.Definitions;

/// <summary>
/// Parameters class usually contains parameters that are required.
/// </summary>
public class Options
{
    /// <summary>
    /// Number of times input is repeated.
    /// </summary>
    /// <example>Example of the parameter value.</example>
    [DefaultValue(3)]
    public int Amount { get; set; }

    /// <summary>
    /// How repeats of the input are separated.
    /// </summary>
    /// <example>Example of the parameter value.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue(" ")]
    public string Delimiter { get; set; }
}

