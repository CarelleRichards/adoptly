using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;

namespace Adoptly.Web.Models;

public class ChangePasswordViewModel
{
    [DefaultValue(true)]
    public string Email { get; init; }

    [DisplayName("New password")]
    [RequiredNotEmpty(ErrorMessage = "You must enter a password.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must have at least eight characters, including an uppercase letter, lowercase letter, number and special character.")]
    public string Password { get; set; }

    [RequiredNotEmpty(ErrorMessage = "You must enter a password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [DisplayName("Confirm new password")]
    public string ConfirmPassword { get; set; }
}