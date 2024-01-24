using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;

namespace Adoptly.Web.Models;

public class EditPasswordViewModel
{
    [RequiredNotEmpty(ErrorMessage = "You must enter your current password.")]
    [DataType(DataType.Password)]
    [DisplayName("Old password")]
    public string OldPassword { get; set; }

    [RequiredNotEmpty(ErrorMessage = "You must enter a new password.")]
    [DataType(DataType.Password)]
    [DisplayName("New password")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must have at least eight characters, including an uppercase letter, lowercase letter, number and special character.")]
    public string NewPassword { get; set; }

    [RequiredNotEmpty(ErrorMessage = "You must re-enter your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [DisplayName("Confirm new password")]
    public string ConfirmPassword { get; set; }
}