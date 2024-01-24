using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;

namespace Adoptly.Web.Models;

public class UpdateEmailViewModel
{
    [RequiredNotEmpty(ErrorMessage = "You must enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid email.")]
    [StringLength(320, ErrorMessage = "Cannot be more than 320 characters.")]
    public string Email { get; set; }
}