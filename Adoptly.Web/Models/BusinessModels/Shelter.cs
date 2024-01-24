using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class Shelter : User
{
    [Required(ErrorMessage = "You must enter shelter name.")]
    [StringLength(30, ErrorMessage = "Must be 30 characters or less.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "Must only contain letters, spaces, commas and periods.")]
    public string Name { get; set; }

    [InverseProperty("Shelter")]
    public virtual List<Pet> Pets { get; set; } = new();

    [Required(ErrorMessage = "Must select a state.")]
    public State State { get; set; }
}