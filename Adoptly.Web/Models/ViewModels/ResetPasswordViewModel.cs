using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;

namespace Adoptly.Web.Models;

public class ResetPasswordViewModel
{
    [RequiredNotEmpty(ErrorMessage = "You must enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
}