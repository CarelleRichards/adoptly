using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Attributes;

public class RequiredNotEmptyAttribute : RequiredAttribute
{
    // Returns false if object is null or if object is a string and empty.

    public override bool IsValid(object value)
    {
        return value is not string
            ? base.IsValid(value)
            : !string.IsNullOrEmpty((string)value);
    }
}