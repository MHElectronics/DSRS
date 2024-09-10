using System.ComponentModel.DataAnnotations;

namespace BOL.Helpers
{
    public class MaxValueAttribute : ValidationAttribute
    {
        private readonly int _maxValue;

        public MaxValueAttribute(int maxValue)
        {
            _maxValue = maxValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int intValue && intValue > _maxValue)
            {
                return new ValidationResult($"The field {validationContext.DisplayName} must be less than or equal to {_maxValue}.");
            }

            if (value is uint uintValue && uintValue > _maxValue)
            {
                return new ValidationResult($"The field {validationContext.DisplayName} must be less than or equal to {_maxValue}.");
            }

            return ValidationResult.Success;
        }
    }
}
