using System.ComponentModel.DataAnnotations;

namespace BOL.Helpers
{
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly int _minValue;

        public MinValueAttribute(int minValue)
        {
            _minValue = minValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int intValue && intValue < _minValue)
            {
                return new ValidationResult($"The field {validationContext.DisplayName} must be greater than or equal to {_minValue}.");
            }

            if (value is uint uintValue && uintValue < _minValue)
            {
                return new ValidationResult($"The field {validationContext.DisplayName} must be greater than or equal to {_minValue}.");
            }

            return ValidationResult.Success;
        }
    }
}
