using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class ApplicationFormViewModel
{
    [Required]
    public int PetId { get; init; }
    
    [Required]
    public string PetName { get; init; }
    
    [Required]
    public string ShelterName { get; init; }
    
    [Required(ErrorMessage = "You must enter your first name.")]
    [StringLength(30, ErrorMessage = "Must be 30 characters or less.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "Must only contain letters, spaces, commas and periods.")]
    [DisplayName("First name")]
    public string FirstName { get; init; }

    [Required(ErrorMessage = "You must enter your last name.")]
    [DisplayName("Last name")]
    [StringLength(30, ErrorMessage = "Must be 30 characters or less.")]
    [RegularExpression(@"^[a-zA-Z ,.'-]+$", ErrorMessage = "Must only contain letters, spaces, commas and periods.")]
    public string LastName { get; init; }
    
    [RequiredNotEmpty(ErrorMessage = "You must enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid email.")]
    [StringLength(320, ErrorMessage = "Cannot be more than 320 characters.")]
    public string Email { get; init; }
    
    [Required(ErrorMessage = "You must enter your street address.")]
    [StringLength(120, ErrorMessage = "Must be 120 characters or less.")]
    [DisplayName("Address line 1")]
    public string Address1 { get; set; }

    [StringLength(120, ErrorMessage = "Must be 120 characters or less.")]
    [DisplayName("Address line 2")]
    public string Address2 { get; set; } = null;
    
    [Required(ErrorMessage = "You must enter a city.")]
    [StringLength(120, ErrorMessage = "Must be 120 characters or less.")]
    [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Please enter enter a city.")]
    public string City { get; set; }
    
    [Required(ErrorMessage = "You must select a state.")]
    public State? State { get; set; }
    
    [Required(ErrorMessage = "You must enter a 4 digit Australian postcode.")]
    [StringLength(4)]
    [RegularExpression("^(0[289][0-9]{2})|([1-9][0-9]{3})$", ErrorMessage = "You must enter a valid Australian post code.")]
    public string Postcode { get; set; }

    [Required]
    [StringLength(9)]
    public string Country { get; init; } = "Australia";
    
    [Required(ErrorMessage = "You must enter a valid Australian contact number.")]
    [DisplayName("Contact number")]
    [StringLength(20)]
    [RegularExpression(@"^\({0,1}((0|\+61)(2|4|3|7|8)){0,1}\){0,1}(\ |-){0,1}[0-9]{2}(\ |-){0,1}[0-9]{2}(\ |-){0,1}[0-9]{1}(\ |-){0,1}[0-9]{3}$", ErrorMessage = "You must enter a valid Australian contact number.")]
    public string ContactNumber { get; set; }

    [Required(ErrorMessage = "You must enter your reason for adopting a pet.")]
    [StringLength(320, ErrorMessage = "Must be 320 characters or less.")]
    [DisplayName("What is your reason for adopting?")]
    public string AdoptionReason { get; set; }
}