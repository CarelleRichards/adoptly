using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class Adopter : User
{
    [Required(ErrorMessage = "You must enter your first name.")]
    [StringLength(30, ErrorMessage = "Must be 30 characters or less.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "Must only contain letters, spaces, commas and periods.")]
    [DisplayName("First name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "You must enter your last name.")]
    [DisplayName("Last name")]
    [StringLength(30, ErrorMessage = "Must be 30 characters or less.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "Must only contain letters, spaces, commas and periods.")]
    public string LastName { get; set; }

    public State? State { get; set; }

    [InverseProperty("Adopter")]
    public virtual Quiz Quiz { get; set; }

    [InverseProperty("Adopter")]
    public virtual List<Match> Matches { get; set; } = new();
    
    [InverseProperty("Adopter")]
    public virtual List<Application> Applications { get; set; } = new();
}