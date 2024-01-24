using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Adoptly.Web.Models;

[BindProperties]
public class CreatePetProfileViewModel
{
    public Shelter Shelter { get; set; }
    
    public string ShelterEmail { get; set; }

    [Required(ErrorMessage = "You must enter a name.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "You must enter a name.")]
    [StringLength(30)]
    public string Name { get; set; }

    [DisplayName("Animal type")]
    [Required(ErrorMessage = "You must select an animal type.")]
    public AnimalType? AnimalType { get; set; }

    [Required(ErrorMessage = "You must enter a description.")]
    [RegularExpression(@"^.{60,}$", ErrorMessage = "You must enter at least 60 characters.")]
    [StringLength(500)]
    public string Description { get; set; }

    [Required(ErrorMessage = "You must select a gender.")]
    public Sex? Sex { get; set; }
    
    [Required(ErrorMessage = "You must enter pet age.")]
    [Range(1, 30, ErrorMessage = "You must enter a valid age.")]
    public float Age { get; set; }

    [Required(ErrorMessage = "You must enter a breed.")]
    [StringLength(100)]
    public string Breed { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "You must select a colour.")]
    public Colour? Colour { get; set; }

    [Required(ErrorMessage = "You must specify if your pet is desexed.")]
    public bool? Desexed { get; set; }

    [DisplayName("Allergy friendly")]
    [Required(ErrorMessage = "You must specify if your pet is allergy friendly.")]
    public bool? AllergyFriendly { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "You must specify pet independence.")]
    public ValueScale? Independence { get; set; }

    [DisplayName("Activity level")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "You must specify pet activity level.")]
    public ValueScale? ActivityLevel { get; set; }
    
    [Required(ErrorMessage = "You must specify pet budget.")]
    public ValueScale? Budget { get; set; }
    
    public IFormFile ImageFile { get; set; }
    public string ProfilePicture { get; set; }
}