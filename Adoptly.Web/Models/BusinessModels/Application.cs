using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adoptly.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Adoptly.Web.Models;

[PrimaryKey(nameof(AdopterUsername), nameof(PetId))]
public class Application
{
    [ForeignKey("Adopter")]
    public string AdopterUsername { get; init; }
    [DeleteBehavior(DeleteBehavior.ClientSetNull)]
    public virtual Adopter Adopter { get; init; }

    [ForeignKey(nameof(PetId))]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PetId { get; init; }
    [DeleteBehavior(DeleteBehavior.ClientSetNull)]
    public virtual Pet Pet { get; init; }
    
    [Required]
    [ForeignKey(nameof(AddressId))]
    public int AddressId { get; init; }
    public virtual Address Address { get; init; }

    [Required]
    [StringLength(320, ErrorMessage = "Must be 320 characters or less.")]
    [DisplayName("What is your reason for adopting?")]
    public string AdoptionReason { get; set; }
    
    [Required]
    [Display(Name = "Application status")]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Received;

    [Required]
    public bool Archived { get; set; } = false;

    [Required]
    [Display(Name = "Application date")]
    public DateTime DateApplied { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Process date")]
    public DateTime? DateProcessed { get; set; } = null;

    [Required]
    public bool Visited { get; set; } = false;
}