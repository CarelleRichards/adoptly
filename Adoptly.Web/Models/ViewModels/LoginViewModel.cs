using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;

namespace Adoptly.Web.Models;

public class LoginViewModel
{
    [RequiredNotEmpty(ErrorMessage = "You must enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [RequiredNotEmpty(ErrorMessage = "You must enter a password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool Admin { get; set; }
}