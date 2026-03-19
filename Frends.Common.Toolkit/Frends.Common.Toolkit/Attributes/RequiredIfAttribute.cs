using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Frends.Common.Toolkit.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class RequiredIfAttribute(string dependentProperty, params object[] targetValues) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var field = validationContext.ObjectType.GetProperty(dependentProperty);
        if (field == null)
            return new ValidationResult($"Unknown property: {dependentProperty}");

        var dependentValue = field.GetValue(validationContext.ObjectInstance);

        if (!targetValues.Contains(dependentValue)) return ValidationResult.Success;
        if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
        }

        return ValidationResult.Success;
    }
}
