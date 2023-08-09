using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Template.Task.Definitions;


/// <summary>
/// Parameters class usually contains parameters that are required.
/// </summary>
public class Input
{
    /// <summary>
    /// Something that will be repeated.
    /// </summary>
    /// <example>Some example of the expected value.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("Lorem ipsum dolor sit amet.")]
    public string Content { get; set; }
}

