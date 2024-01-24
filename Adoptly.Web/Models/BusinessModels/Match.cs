using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Adoptly.Web.Models;

[PrimaryKey(nameof(AdopterUsername), nameof(PetId))]
public class Match
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
    public double Score { get; set; }

    [Required]
    [DisplayName("Date matched")]
    public DateTime DateMatched { get; set; } = DateTime.UtcNow;

    public int MatchmakingPercent(double total) => (int)Math.Round((double)(100 * Score) / total);

    public bool IsEqual(Match match)
    {
        if (AdopterUsername != match.AdopterUsername)
            return false;
        if (PetId != match.PetId)
            return false;
        if (Score != match.Score)
            return false;
        return true;
    }
}