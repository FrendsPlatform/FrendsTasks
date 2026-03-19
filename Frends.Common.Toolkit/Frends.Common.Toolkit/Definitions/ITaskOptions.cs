using System.ComponentModel;

namespace Frends.Common.Toolkit.Definitions;

public interface ITaskOptions
{
    [DefaultValue(true)]
    bool ThrowErrorOnFailure { get; set; }
    [DefaultValue("")]
    string ErrorMessageOnFailure { get; set; }
}
