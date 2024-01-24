using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models;

public class AdminReportViewModel
{
    [DisplayFormat(DataFormatString = "{0:%d} D, {0:%h} H, {0:%m} M, {0:%s} S", ApplyFormatInEditMode = true)]
    [Display(Name = "Average adoption time")]
    public TimeSpan AverageAdoptionTime { get; set; }

    [Display(Name = "Pets available")]
    public int PetsAvailable { get; set; }

    [Display(Name = "Pets adopted")]
    public int PetsAdopted { get; set; }

    [Display(Name = "Dogs available")]
    public int DogsAvailable { get; set; }

    [Display(Name = "Cats available")]
    public int CatsAvailable { get; set; }

    [Display(Name = "Adopter accounts")]
    public int AdopterAccounts { get; set; }

    [Display(Name = "Shelter accounts")]
    public int ShelterAccounts { get; set; }

    [Display(Name = "Monthly adoptions")]
    public string MonthlyAdoptions { get; set; }
}