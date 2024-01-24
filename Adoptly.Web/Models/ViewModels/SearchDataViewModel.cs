
using Azure.Search.Documents.Models;
using Adoptly.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models;

public class SearchDataViewModel
{
    public string Term { get; set; }

    public SearchResults<PetSearchResult> ResultList;

    [Display(Name = "Animal type")]
    public List<AnimalType> AnimalType { get; set; } = new();

    public List<State> State { get; set; } = new();

    public List<Sex> Sex { get; set; } = new();

    public List<Status> Status { get; set; } = new();

    public List<Colour> Colour { get; set; } = new();

    public bool Desexed { get; set; }

    [Display(Name = "Allergy friendly")]
    public bool AllergyFriendly { get; set; }

    public double MinAge { get; set; } = 0;

    public double MaxAge { get; set; } = 10;

    [Display(Name = "Sort")]
    public SortOrder? SortOrder { get; set; }
}