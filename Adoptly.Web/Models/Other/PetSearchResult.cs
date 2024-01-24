using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;

namespace Adoptly.Web.Models;

// This model represents the index used in Azure Cognitive Search.

public class PetSearchResult
{
    [SimpleField(IsKey = true)]
    public string Id { get; set; }

    public string ShelterUsername { get; set; }

    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Name { get; set; }

    [SimpleField(IsFilterable = true)]
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string AnimalType { get; set; }

    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Description { get; set; }

    public string ProfilePicture { get; set; }

    [SimpleField(IsFilterable = true)]
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string State { get; set; }

    [SimpleField(IsSortable = true)]
    public DateTime FirstListed { get; set; }

    [SimpleField(IsFilterable = true)]
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Sex { get; set; }

    [SimpleField(IsFilterable = true)]
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Status { get; set; }

    [SimpleField(IsFilterable = true)]
    public double Age { get; set; }

    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Breed { get; set; }

    [SimpleField(IsFilterable = true)]
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Colour { get; set; }

    [SimpleField(IsFilterable = true)]
    public bool Desexed { get; set; }

    [SimpleField(IsFilterable = true)]
    public bool AllergyFriendly { get; set; }

    public string Independence { get; set; }

    public string ActivityLevel { get; set; }

    public string Budget { get; set; }
}