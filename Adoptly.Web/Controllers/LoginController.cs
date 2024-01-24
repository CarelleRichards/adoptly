using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Models;
using Adoptly.Web.Filters;
using Adoptly.Web.Managers;
using SimpleHashing.Net;

namespace Adoptly.Web.Controllers;

public class LoginController : Controller
{
    private readonly LoginManager _loginManager;
    private readonly AdopterManager _adopterManager;
    private readonly ShelterManager _shelterManager;
    private readonly AdminManager _adminManager;

    private static readonly ISimpleHash SimpleHash = new SimpleHash();

    public LoginController(LoginManager loginManager, AdopterManager adopterManager,
        AdminManager adminManager, ShelterManager shelterManager)
    {
        _loginManager = loginManager;
        _shelterManager = shelterManager;
        _adopterManager = adopterManager;
        _adminManager = adminManager;
    }

    [ReturnHomeIfLoggedIn]
    public IActionResult Index() => View(new LoginViewModel());

    [ReturnHomeIfLoggedIn]
    public IActionResult Admin() => View(nameof(Index), new LoginViewModel() { Admin = true });

    [HttpPost]
    [ReturnHomeIfLoggedIn]
    public IActionResult Submit(LoginViewModel viewModel)
    {        
        if (!ModelState.IsValid)
            return View(nameof(Index), viewModel);

        Login login = Login(viewModel.Email, viewModel.Password);

        // Check if login details exist.

        if (login is null || login.User is Admin && !viewModel.Admin ||
            viewModel.Admin && (login.User is Adopter || login.User is Shelter))
        {
            ModelState.AddModelError("Failure", "Incorrect email or password, please try again.");
            return View(nameof(Index), viewModel);
        }

        // Check if login details are verified.

        if (!login.Verified)
        {
            ViewBag.Verification =
                $"<a class=\"adoptly-link\" href=\"/SignUp/ResendVerification?email={login.Email}\">Resend email</a>";
            ModelState.AddModelError("Verification", $"Check your emails to verify your login.");
            return View(nameof(Index), viewModel);
        }

        // If login details exists and are verified, create session.

        string role = login.User is Admin ? "Admin" : login.User is Adopter ? "Adopter" : "Shelter";

        HttpContext.Session.SetString("Username", login.User.Username);
        HttpContext.Session.SetString("Role", role);
        
        return RedirectToAction("Index", role);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // Returns a Login object if the given email and username are in the system. 

    private Login Login(string email, string password)
    {
        Login login = _loginManager.GetByEmail(email);
        return login is null || !SimpleHash.Verify(password, login.PasswordHash) ? null : login;
    }
}