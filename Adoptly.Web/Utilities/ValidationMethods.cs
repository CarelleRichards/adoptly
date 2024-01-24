using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Utilities;

public class ValidationMethods
{
    // Validates an object using their data annotations.

    public static bool Validate<T>(T model, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, new ValidationContext(model), results, true);
    }
}