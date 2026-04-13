using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Company.Echo.Toolkit.Tests.AttributesTests;

public abstract class AttributeTestBase
{
    protected static List<ValidationResult> Validate(object obj)
    {
        var ctx = new ValidationContext(obj);
        List<ValidationResult> validateResults = [];
        Validator.TryValidateObject(obj, ctx, validateResults, true);

        return validateResults;
    }
}
