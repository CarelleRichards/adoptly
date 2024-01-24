using System.ComponentModel;

namespace Adoptly.Web.Models;

public class AdopterDashboardViewModel
{
    [DisplayName("Pets that still need homes")]
    public List<Pet> PetsThatNeedHomes { get; set; } = new();

    [DisplayName("Your top matches")]
    public List<Match> MatchedPets { get; set; } = new();

    public Quiz Quiz { get; set; } = new();
}