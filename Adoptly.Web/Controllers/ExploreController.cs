using Azure.Search.Documents;
using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Azure.Search.Documents.Models;
using System.Text;
using System.Text.RegularExpressions;
using Adoptly.Web.Filters;

namespace Adoptly.Web.Controllers;

[AuthorizeAnyUser]
public class ExploreController : Controller
{
    private const int MinPage = 1;
    private const int PageSize = 6;
    private const int MaxPage = 100000;
    private const int MinFilterAge = 0;
    private const int MaxFilterAge = 10;
    private readonly SearchClient _searchClient;

    public ExploreController(SearchClient searchClient) => _searchClient = searchClient;

    public IActionResult Index() => Redirect("Explore/Search?Term=");

    [HttpGet]
    public async Task<ActionResult> Filter(SearchDataViewModel searchData, int page = 1)
    {
        try
        {
            await RunQueryAsync(searchData, page);
        }
        catch (InvalidDataException)
        {
            return RedirectToAction("Index", "Home");
        }
        return View("Index", searchData);
    }

    [HttpGet]
    public async Task<ActionResult> Search(SearchDataViewModel searchData, int page = 1)
    {
        try
        {
            await RunQueryAsync(searchData, page);
        }
        catch (InvalidDataException)
        {
            return RedirectToAction("Index", "Home");
        }
        return View("Index", searchData);
    }

    // Run query on Azure Cognitive Search. 

    private async Task<ActionResult> RunQueryAsync(SearchDataViewModel searchData, int page)
    {
        if (!ModelState.IsValid || page < MinPage || page > MaxPage ||
            searchData.MinAge >= searchData.MaxAge || searchData.MinAge < 0 || searchData.MaxAge < 1)
            throw new InvalidDataException("Search data or page number is invalid.");

        // If no search term is entered, the sort order is always newest first. 

        if (string.IsNullOrWhiteSpace(searchData.Term))
        {
            searchData.Term = "";
            searchData.SortOrder ??= SortOrder.Newest;
        }

        // Escape special characters and add partial match pattern to search text.

        string finalSearchText = MakePartialMatch(EscapeSpecialCharacters(searchData.Term));

        // Configure options.

        SearchOptions options = new()
        {
            IncludeTotalCount = true,
            QueryType = SearchQueryType.Full,
            SearchMode = SearchMode.All,
            Size = PageSize,
            Skip = (page - 1) * PageSize
        };

        // Add filters to search options. 

        string filters = MakeFilters(searchData);

        if (filters is not null)
            options.Filter = filters;

        // Configure sort order.

        if (searchData.SortOrder == SortOrder.Newest)
            options.OrderBy.Add("FirstListed desc");
        else if (searchData.SortOrder == SortOrder.Oldest)
            options.OrderBy.Add("FirstListed asc");

        // Get search results.

        searchData.ResultList = await _searchClient
            .SearchAsync<PetSearchResult>(finalSearchText, options)
            .ConfigureAwait(false);

        // Configure pagination.

        int totalPages = (int)Math.Ceiling((double)searchData.ResultList.TotalCount / PageSize);

        if (totalPages != 0 && page > totalPages)
            throw new InvalidDataException("Search data or page number is invalid.");

        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;

        return View("Index", searchData);
    }

    // Creates a string with filters from a SearchDataViewModel.

    private static string MakeFilters(SearchDataViewModel searchData)
    {
        List<string> filters = new()
        {
            MakeFilterFromList(searchData.AnimalType, "AnimalType"),
            MakeFilterFromList(searchData.State, "State"),
            MakeFilterFromList(searchData.Sex, "Sex"),
            MakeFilterFromList(searchData.Status, "Status"),
            MakeFilterFromList(searchData.Colour, "Colour")
        };

        if (searchData.Desexed)
            filters.Add("Desexed eq true");
    
        if (searchData.AllergyFriendly)
            filters.Add("AllergyFriendly eq true");

        if (searchData.MinAge != MinFilterAge)
            filters.Add($"Age ge {searchData.MinAge}");

        if (searchData.MaxAge < MaxFilterAge)
            filters.Add($"Age le {searchData.MaxAge}");

        filters.RemoveAll(string.IsNullOrEmpty);

        StringBuilder filtersStr = new();

        if (filters.Count >= 1)
            foreach (string filter in filters)
            {
                if (filter != filters.First())
                    filtersStr.Append(" and ");
                filtersStr.Append(filter);
            }

        return filtersStr.Length > 0 ? filtersStr.ToString() : null;
    }

    // Creates a string with filters from a given list.

    private static string MakeFilterFromList<T>(List<T> list, string filterName)
    {
        StringBuilder filter = null;

        if (list.Any())
        {
            filter = new($"search.in({filterName},\'");

            foreach (T x in list)
                filter.Append($"{x},");

            filter.Length--;
            filter.Append($"')");
        }

        return filter is null ? null : filter.ToString();
    }

    // Escape special characters so app doesn't crash.
    // Note these will be stripped on analysis.     
    // Ref: https://learn.microsoft.com/en-us/azure/search/query-lucene-syntax.

    private static string EscapeSpecialCharacters(string searchText)
    {
        string pattern = @"[-+&|!(){}\[\]^""~*?:\\/]";
        string escapedString = Regex.Replace(searchText, pattern, @"\$&");
        return escapedString;
    }

    // Adds syntax to given searchTest string for wildcard search.

    private static string MakePartialMatch(string searchText)
    {
        string[] words = searchText.Split(' ');
        string pattern = @"\b\w+\b";
        StringBuilder result = new();

        foreach (string word in words)
        {
            string wrappedWord = Regex.Replace(word, pattern, "/.*$0.*/");
            result.Append(wrappedWord + " ");
        }

        string finalResult = result.ToString().Trim();
        return finalResult;
    }
}