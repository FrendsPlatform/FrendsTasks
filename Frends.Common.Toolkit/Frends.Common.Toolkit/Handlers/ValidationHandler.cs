using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Frends.Common.Toolkit.Handlers;

public static class ValidationHandler
{
    public static void Run(params object[] objects)
    {
        var validationMessage = objects.Aggregate(string.Empty, (current, obj) => current + obj.Validate());

        if (validationMessage != string.Empty) throw new ValidationException($"Validation failed:\n{validationMessage}");
    }

    private static string Validate<T>(this T objectToValidate)
    {
        if (objectToValidate == null) return "Validated object can't be null!\n";
        var ctx = new ValidationContext(objectToValidate);
        List<ValidationResult> validateResults = [];
        Validator.TryValidateObject(objectToValidate, ctx, validateResults, true);

        return validateResults.Aggregate(string.Empty, (current, error) => current + $"{error.ErrorMessage}\n");
    }

}
