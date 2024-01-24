using Adoptly.Web.Filters;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Adoptly.Web.Managers;
using Adoptly.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Utilities;
using System.ComponentModel.DataAnnotations;
using SimpleHashing.Net;

namespace Adoptly.Web.Controllers;

public class AdopterController : Controller
{
    private readonly AdopterManager _adopterManager;
    private readonly PetManager _petManager;
    private readonly LoginManager _loginManager;
    private readonly ApplicationManager _applicationManager;
    private readonly AddressManager _addressManager;
    private readonly MatchManager _matchManager;
    private readonly MatchmakerService _matchmakerService;
    private readonly QuizManager _quizManager;
    private readonly FileService _fileService;
    private readonly EmailService _emailService;
    private static readonly ISimpleHash SimpleHash = new SimpleHash();
    private const int PageSize = 6;
    private const int FeaturedPetsCount = 6;
    private const int MatchedPetsCount = 4;

    public AdopterController(AdopterManager adopterManager, PetManager petManager, AddressManager addressManager,
        ApplicationManager applicationManager, LoginManager loginManager, MatchmakerService matchmakerService,
        MatchManager matchManager, QuizManager quizManager, FileService fileService, EmailService emailService)
    {
        _adopterManager = adopterManager;
        _petManager = petManager;
        _loginManager = loginManager;
        _applicationManager = applicationManager;
        _addressManager = addressManager;
        _matchManager = matchManager;
        _quizManager = quizManager;
        _matchmakerService = matchmakerService;
        _fileService = fileService;
        _emailService = emailService;
        _loginManager = loginManager;
    }

    [AuthorizeAdopter]
    public IActionResult Index()
    {
        // Get all pets.
        
        List<Pet> allPets = _petManager.GetAll();
        
        // Get authorised adopter.
        
        Adopter adopter = _adopterManager.GetByUsername(HttpContext.Session.GetString("Username"));
        
        // Get a list of adopter's matches.
        
        List<Match> matches = _matchmakerService.GetMatches(adopter);

        // Create a view model object containing pets and the matched pets and return to the view.
        
        AdopterDashboardViewModel viewModel = new()
        {
            PetsThatNeedHomes = allPets
                .Where(x => x.Status == Status.Available)
                .OrderBy(x => x.FirstListed)
                .Take(FeaturedPetsCount)
                .ToList(),
            MatchedPets = matches
                .Take(MatchedPetsCount)
                .ToList()
        };

        // If the user just submitted the quiz, validate it.
        // If quiz is invalid, reopen the modal with errors.

        if (TempData.ContainsKey("Quiz"))
        {
            viewModel.Quiz = TempData.Get<Quiz>("Quiz");
            ValidationMethods.Validate(viewModel.Quiz, out List<ValidationResult> errors);

            if (errors is not null)
            {
                foreach (ValidationResult e in errors)
                    ModelState.AddModelError(e.MemberNames.First(), e.ErrorMessage);
                ViewBag.OpenModal = true;
            }
        }

        ViewBag.TotalQuestions = _matchmakerService.TotalQuestions;
        ViewBag.HasTakenQuiz = adopter.Quiz is not null;

        return View(viewModel);
    }

    [AuthorizeAnyUser]
    public IActionResult Profile(string id)
    {
        // Get adopter by username.
        
        Adopter adopter = _adopterManager.GetByUsername(id);
        
        // Return adopter to the view.
        
        return adopter is not null ? View(adopter) : RedirectToAction("Index", "Home");
    }

    [AuthorizeAdminOrAdopter]
    public ActionResult EditProfile(string adopterUsername = null)
    {
        // Check if the authorised user is either an admin or an adopter.
        
        Adopter adopter = _adopterManager.GetByUsername(adopterUsername ?? HttpContext.Session.GetString("Username"));

        // Check if the adopter is null and redirect to the home page.
        
        if (adopter is null)
            return RedirectToAction("Index", "Home");

        ViewBag.Email = adopter.Login.Email;

        // Return adopter to the view.
        
        return View(adopter);
    }

    [HttpPost]
    [AuthorizeAdminOrAdopter]
    public async Task<IActionResult> EditProfile(Adopter adopter)
    {
        // Check if a profile picture is provided for the profile.
        
        if (adopter.ImageFile is not null && adopter.ImageFile.Length > 0)
        {
            string profilePicture = $"{adopter.Username}-{Guid.NewGuid()}.jpg";
            try
            {
                // Upload adopter profile picture to Azure's blob storage.
                
                bool uploaded = await _fileService.UploadFile(
                    adopter.ImageFile, profilePicture, BlobContainers.AdopterProfilePictures);

                if (!uploaded)
                    ModelState.AddModelError("ProfilePicture", "Problem uploading profile picture.");
                else
                {
                    // Delete the old profile picture if any exists.
                    
                    if (adopter.ProfilePicture is not null)
                        await _fileService.DeleteFile(adopter.ProfilePicture, BlobContainers.AdopterProfilePictures);
                    adopter.ProfilePicture = profilePicture;
                }
            }
            catch (IOException)
            {
                ModelState.AddModelError("ProfilePicture", "Invalid file type. Upload an image file.");
            }
        }

        if (!ModelState.IsValid)
            return View(adopter);

        // Update adopter.
        
        _adopterManager.Update(adopter);

        return RedirectToAction("Profile", "Adopter", new { id = adopter.Username });
    }

    [HttpPost]
    [AuthorizeAdminOrAdopter]
    public async Task<ActionResult> DeleteProfilePicture(string username = null)
    {
        // Check if the authorised user is either an admin or an adopter.
        
        Adopter adopter = _adopterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

        // Check if the user does have a profile picture.
        
        if (adopter.ProfilePicture is not null)
        {
            // Delete user profile picture.
            
            await _fileService.DeleteFile($"{adopter.Username}-profile-picture.jpg",
                BlobContainers.AdopterProfilePictures);
            adopter.ProfilePicture = null;
            _adopterManager.Update(adopter);
        }

        return RedirectToAction("Profile", "Adopter", new { id = adopter.Username });
    }

    [HttpPost]
    [AuthorizeAdminOrAdopter]
    public async Task<IActionResult> UpdateEmail(UpdateEmailViewModel updateEmailViewModel, string username = null)
    {
        ViewBag.DisplaySecurityTab = true;

        // Check if the authorised user is either an admin or an adopter.

        Adopter adopter = _adopterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

        // Check if the entered email is the same in the database.

        if (adopter.Login.Email == updateEmailViewModel.Email)
            ModelState.AddModelError("Email", "Please enter a new email address to update your account.");

        // Check if the entered email does not belong to another user.

        if (_loginManager.GetByEmail(updateEmailViewModel.Email) is not null)
            ModelState.AddModelError("Email", "Email already exists.");

        if (!ModelState.IsValid)
            return View(nameof(EditProfile), adopter);

        // Send verification email.
        
        await _emailService.SendEmailAsync(
            updateEmailViewModel.Email, EmailType.ChangeEmail, adopter.Login.VerificationToken, adopter.Login.Email);

        ViewBag.DisplayEmailSuccess = true;

        return View(nameof(EditProfile), adopter);
    }

    [AuthorizeAnyUser]
    public ActionResult ProfilePicture(string id)
    {
        Adopter adopter = _adopterManager.GetByUsername(id);

        // Display default profile picture.

        if (adopter is null || adopter.ProfilePicture is null)
            return RedirectToAction("Get", "Images", new
            {
                containerName = "brand-assets",
                imgName = "adopter-default-profile-picture.png",
                contentType = "img/png"
            });

        // Display user's profile picture.

        return RedirectToAction("Get", "Images", new
        {
            containerName = "adopter-profile-pictures",
            imgName = adopter.ProfilePicture,
            contentType = "img/jpg"
        });
    }

    [HttpPost]
    [AuthorizeAdminOrAdopter]
    public IActionResult EditPassword(EditPasswordViewModel viewModel, string username = null)
    {
        // Check if the authorised user is either an admin or an adopter.
        
        Adopter adopter = _adopterManager.GetByUsername(username ?? HttpContext.Session.GetString("Username"));

        ViewBag.Email = adopter.Login.Email;
        ViewBag.DisplaySecurityTab = true;

        // Check if the user is an adopter.

        if (username is null)
        {
            if (viewModel.OldPassword == null | viewModel.NewPassword == null | viewModel.ConfirmPassword == null)
                return View(nameof(EditProfile), adopter);

            // Check if the old password entered is correct.

            bool passwordCorrect = SimpleHash.Verify(viewModel.OldPassword, adopter.Login.PasswordHash);
            
            // Check if new and confirm passwords match.

            bool passwordMatch = viewModel.NewPassword == viewModel.ConfirmPassword;

            if (!passwordCorrect)
                ModelState.AddModelError("OldPassword", "Incorrect current password.");
        }

        // Remove old password from the model state if the user is an admin.

        if (username is not null)
            ModelState.Remove("OldPassword");

        if (!ModelState.IsValid)
            return View(nameof(EditProfile), adopter);

        // Update password.

        adopter.Login.PasswordHash = SimpleHash.Compute(viewModel.NewPassword);
        _loginManager.Update(adopter.Login);

        ViewBag.DisplayPasswordSuccess = true;

        return View(nameof(EditProfile), adopter);
    }

    [AuthorizeAdopter]
    public ActionResult Applications(int page = 1)
    {
        List<Application> applications = _applicationManager
            .GetNonArchivedByAdopterUsername(HttpContext.Session.GetString("Username"));

        // Check if the authorised user is the adopter user who owns the applications.

        if (applications is null)
            return RedirectToAction("Index", "Adopter");

        int totalPages = (int)Math.Ceiling((double)applications.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");

        // Paginate applications and order by date applied.

        applications = applications
            .OrderByDescending(x => x.DateApplied)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.TotalResults = applications.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return applications to the view.
        
        return View(applications);
    }

    [AuthorizeAdopter]
    public ActionResult ArchivedApplications(int page = 1)
    {
        List<Application> applications = _applicationManager
            .GetArchivedByAdopterUsername(HttpContext.Session.GetString("Username"));

        // Check if the authorised user is the adopter user who owns the applications.

        if (applications is null)
            return RedirectToAction("Index", "Adopter");

        int totalPages = (int)Math.Ceiling((double)applications.Count / PageSize);

        if (totalPages != 0 && page > totalPages)
            return RedirectToAction("Index", "Home");
        
        // Paginate applications and order by date applied.

        applications = applications
            .OrderByDescending(x => x.DateApplied)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.TotalResults = applications.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        // Return applications to the view.

        return View(applications);
    }

    [AuthorizeAdopter]
    public ActionResult Application(int petId)
    {
        Adopter adopter = _adopterManager.GetByUsername(HttpContext.Session.GetString("Username"));

        // Check if the authorised adopter user is accessing the application.

        if (adopter is null)
            return RedirectToAction("Index", "Adopter");

        // Get application by adopter username and pet id.
        
        Application application = _applicationManager.Get(adopter.Username, petId);
        application.Visited = true;
        _applicationManager.Update(application);

        return View(application);
    }

    [HttpPost]
    [AuthorizeAdopter]
    public ActionResult DeleteApplication(Application application)
    {
        string id = HttpContext.Session.GetString("Username");
        Adopter adopter = _adopterManager.GetByUsername(id);

        // Check if the authorised adopter user is accessing the application.

        if (adopter is null)
            return RedirectToAction("Index", "Adopter");

        // Delete the application along with the application address.
        
        _applicationManager.Delete(_applicationManager.Get(id, application.PetId));
        _addressManager.Delete(_addressManager.GetById(application.AddressId));

        return RedirectToAction("Applications", "Adopter");
    }

    [HttpPost]
    [AuthorizeAdminOrAdopter]
    public async Task<ActionResult> DeleteAccount(Adopter adopter, string username = null)
    {
        string id = null;

        // Check if the authorised user is either an admin or an adopter.

        if (HttpContext.Session.GetString("Role") == nameof(Adopter))
            id = HttpContext.Session.GetString("Username");
        else if (HttpContext.Session.GetString("Role") == nameof(Admin))
            id = username;

        if (adopter.Username != id)
            return RedirectToAction("Index", "Adopter");

        // Delete the adopter's applications address.

        foreach (Application application in _applicationManager.GetByAdopterUsername(id))
            _addressManager.Delete(_addressManager.GetById(application.AddressId));

        // Delete applications, matches and quizzes associated with the adopter account.

        _applicationManager.DeleteAllByAdopterUsername(id);
        _matchManager.DeleteAllByAdopterUsername(id);
        _quizManager.Delete(_quizManager.Get(id));

        // Delete adopter's profile picture form Azure's blob storage.
        
        if (adopter.ProfilePicture is not null)
            await _fileService.DeleteFile(adopter.ProfilePicture, BlobContainers.AdopterProfilePictures);

        // Delete adopter's login.

        _loginManager.Delete(_loginManager.GetByUsername(id));

        return HttpContext.Session.GetString("Role") == nameof(Admin)
            ? RedirectToAction("Users", "Admin")
            : RedirectToAction("Logout", "Login");
    }
}