using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SWMS.Influx.Module.Attributes
{
    public class DateBeforeAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateBeforeAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the type of the object being validated
            var objectType = validationContext.ObjectInstance.GetType();

            // Get the property information for the comparison property
            var comparisonProperty = objectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            // Get the value of the comparison property
            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);

            if (comparisonValue == null)
            {
                return ValidationResult.Success; // Consider null as valid; adjust as needed
            }

            if (!(value is DateTime currentValue))
            {
                return new ValidationResult("The current value is not a valid date.");
            }

            if (!(comparisonValue is DateTime comparisonDate))
            {
                return new ValidationResult($"The comparison value ({_comparisonProperty}) is not a valid date.");
            }

            if (currentValue >= comparisonDate)
            {
                return new ValidationResult(ErrorMessage ?? $"The date must be before {comparisonDate.ToShortDateString()}.");
            }

            return ValidationResult.Success;
        }
    }
}
