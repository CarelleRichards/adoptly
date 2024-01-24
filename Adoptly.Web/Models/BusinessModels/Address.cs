using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class Address
{
    [Key]
    public int Id { get; init; }
    
    [Required(ErrorMessage = "You must enter your street address.")]
    [StringLength(120, ErrorMessage = "Must be 120 characters or less.")]
    [DisplayName("Address line 1")]
    public string Address1 { get; set; }

    [StringLength(120, ErrorMessage = "Must be 120 characters or less.")]
    [DisplayName("Address line 2")]
    public string Address2 { get; set; } = null;
    
    [Required(ErrorMessage = "You must enter a city.")]
    [StringLength(120, ErrorMessage = "Must be 120 characters or less.")]
    [RegularExpression("^[a-zA-Z ]*$")]
    public string City { get; set; }
    
    [Required(ErrorMessage = "You must select a state.")]
    public State State { get; set; }
    
    [Required(ErrorMessage = "You must enter a post code.")]
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
}