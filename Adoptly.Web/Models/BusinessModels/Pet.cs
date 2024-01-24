using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class Pet
{
    [Key]
    public int Id { get; init; }

    [Required]
    [ForeignKey("Username")]
    public string ShelterUsername { get; init; }
    public virtual Shelter Shelter { get; init; }
    
    [InverseProperty("Pet")]
    public virtual List<Application> Applications { get; set; } = new();

    [Required(ErrorMessage = "You must enter a name.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "Must only contain letters, spaces, commas and periods.")]
    [StringLength(30)]
    public string Name { get; set; }

    [Required(ErrorMessage = "You must select an animal type.")]
    [Display(Name = "Animal type")]
    public AnimalType AnimalType { get; set; }

    [Required(ErrorMessage = "You must enter a description.")]
    [RegularExpression(@"^.{60,}$", ErrorMessage = "You must enter at least 60 characters.")]
    [StringLength(500)]
    public string Description { get; set; }

    [StringLength(70)]
    public string ProfilePicture { get; set; }

    [Required]
    public State State { get; set; }

    [Required]
    [Display(Name = "Date listed")]
    public DateTime FirstListed { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "You must select a gender.")]
    public Sex Sex { get; set; }

    private Status _status;

    [Required(ErrorMessage = "You must specify a status.")]
    public Status Status 
    {
        get { return _status; }
        set
        {
            _status = value;
            StatusLastUpdated = DateTime.UtcNow;
        }
    }

    [Required]
    [Display(Name = "Status last updated")]
    public DateTime StatusLastUpdated { get; set; }

    [Required(ErrorMessage = "You must enter pet age.")]
    [Range(1, 30, ErrorMessage = "You must enter a valid age.")]
    public double Age { get; set; }

    [Required(ErrorMessage = "You must enter a breed.")]
    [StringLength(100)]
    public string Breed { get; set; }

    [Required(ErrorMessage = "You must select a colour.")]
    public Colour Colour { get; set; }

    [Required(ErrorMessage = "You must specify if your pet is desexed.")]
    public bool Desexed { get; set; }

    [Required(ErrorMessage = "You must specify if your pet is allergy friendly.")]
    [Display(Name = "Allergy friendly")]
    public bool AllergyFriendly { get; set; }

    [Required(ErrorMessage = "You must specify pet independence.")]
    public ValueScale Independence { get; set; }

    [Required(ErrorMessage = "You must specify pet activity level.")]
    [Display(Name = "Activity level")]
    public ValueScale ActivityLevel { get; set; }

    [Required(ErrorMessage = "You must specify pet budget.")]
    public ValueScale Budget { get; set; }
    
    [NotMapped]
    public IFormFile ImageFile { get; set; }

    [InverseProperty("Pet")]
    public virtual List<Match> Matches { get; set; } = new();
}