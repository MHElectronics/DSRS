using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BOL.Helpers
{
    public class NoSpecialCharacters : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string stringValue)
            {
                // Regular expression to match any special character
                if (Regex.IsMatch(stringValue, @"[^a-zA-Z0-9\s]"))
                {
                    return new ValidationResult($"The field {validationContext.DisplayName} must not contain special characters.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
