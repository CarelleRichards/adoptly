using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models;

public class Admin : User
{
    [Required]
    [StringLength(30)]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "You must enter your name.")]
    [DisplayName("First name")]
    public string FirstName { get; set; }

    [Required]
    [StringLength(30)]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "You must enter your lastname.")]
    [DisplayName("First name")]
    public string LastName { get; set; }
}