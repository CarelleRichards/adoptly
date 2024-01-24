using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Filters;
using Adoptly.Web.Utilities;

namespace Adoptly.Web.Controllers;

[AuthorizeAdopter]
public class MatchmakerController : Controller
{
    private readonly AdopterManager _adopterManager;
    private readonly MatchmakerService _matchmakerService;
    private const int PageSize = 6;

    public MatchmakerController(AdopterManager adopterManager, MatchmakerService matchmakerService)
    {
        _adopterManager = adopterManager;
        _matchmakerService = matchmakerService;
    }

    public IActionResult Index() => View(new Quiz());

    [HttpPost]
    public IActionResult Submit(Quiz viewModel)
    {
        ModelState.Remove("AdopterEmail");

        // If quiz answers are not valid, show errors.

        if (!ModelState.IsValid)
        {
            TempData.Put("Quiz", viewModel);
            return RedirectToAction("Index", "Adopter", null);
        }

        // If quiz answers are valid, update database and show results page.
            
        Adopter adopter = _adopterManager.GetByUsername(HttpContext.Session.GetString("Username"));
        _matchmakerService.UpdateQuiz(adopter, viewModel);

        return RedirectToAction(nameof(Results), new { page = 1 });
    }

    public IActionResult Results(int page)
    {
        if (page < PaginationViewModel.StartPage)
            return RedirectToAction("Index", "Home");

        // Get pet matches for user.

        Adopter adopter = _adopterManager.GetByUsername(HttpContext.Session.GetString("Username"));
        List<Match> matches = _matchmakerService.GetMatches(adopter);

        // Paginate match results.

        int totalPages = (int)Math.Ceiling((double)matches.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        List<Match> pagedMatches = matches.Skip((page - 1) * PageSize).Take(PageSize).ToList();

        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.TotalMatches = matches.Count;
        ViewBag.TotalQuestions = _matchmakerService.TotalQuestions;

        return View(pagedMatches);
    }
}