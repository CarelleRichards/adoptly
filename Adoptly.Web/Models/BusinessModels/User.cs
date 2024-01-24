using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Adoptly.Web.Models;

[Index(nameof(LoginId), IsUnique = true)]
public abstract class User
{
    [Key]
    [StringLength(30)]
    [RegularExpression(@"^[a-zA-Z0-9]+([_ -]?[a-zA-Z0-9]){2,}$")]
    public string Username { get; set; }

    [Required]
    [ForeignKey("Id")]
    public int LoginId { get; set; }
    public virtual Login Login { get; set; }

    [StringLength(70)]
    public string ProfilePicture { get; set; }
    
    [NotMapped]
    public IFormFile ImageFile { get; set; }
}