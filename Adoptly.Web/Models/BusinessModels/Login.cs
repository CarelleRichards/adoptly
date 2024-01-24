using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Adoptly.Web.Models;

[Index(nameof(Email), IsUnique = true)]
public class Login
{
    [Key]
    [InverseProperty("User")]
    public int Id { get; set; }
    public virtual User User { get; set; }    

    [Required]
    [StringLength(320)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(94)]
    public string PasswordHash { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    private bool _verified = false;

    public bool Verified
    {
        get { return _verified; }
        set
        {
            _verified = value;
            if (!_verified)
                DateVerified = null;
            else
                DateVerified = DateTime.UtcNow;
        }
    }

    public DateTime? DateVerified { get; set; } 

    [Required]
    [StringLength(36)]
    public string VerificationToken { get; set; } = Guid.NewGuid().ToString();
}