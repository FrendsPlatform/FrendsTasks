using System.ComponentModel.DataAnnotations;

namespace Frends.Common.Toolkit.Attributes;

public class NotEmptyStringAttribute : RequiredAttribute
{
    public NotEmptyStringAttribute()
    {
        AllowEmptyStrings = false;
        ErrorMessage = "{0} is required and cannot be empty.";
    }
}
