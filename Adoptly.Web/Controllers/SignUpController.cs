using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Models;
using Adoptly.Web.Services;
using Adoptly.Web.Managers;
using Adoptly.Web.Models.Enums;
using SimpleHashing.Net;

namespace Adoptly.Web.Controllers;

public class SignUpController : Controller
{
    private readonly EmailService _emailService;
    private readonly LoginManager _loginManager;
    private readonly ShelterManager _shelterManager;
    private readonly AdminManager _adminManager;
    private readonly AdopterManager _adopterManager;
    private static readonly ISimpleHash SimpleHash = new SimpleHash();

    public SignUpController(EmailService emailService, LoginManager loginManager,
        AdminManager adminManager, AdopterManager adopterManager, ShelterManager shelterManager)
    {
        _emailService = emailService;
        _loginManager = loginManager;
        _adminManager = adminManager;
        _shelterManager = shelterManager;
        _adopterManager = adopterManager;
    }

    public IActionResult Index() => View(new SignUpViewModel());

    [HttpPost]
    public async Task<IActionResult> Submit(SignUpViewModel viewModel)
    {
        // Check if email exists in the database.
        
        if (_loginManager.GetByEmail(viewModel.Email) is not null)
            ModelState.AddModelError("Email", "Email already exists.");

        // Check if username exists in the database.
        
        if (_loginManager.GetByUsername(viewModel.Username) is not null)
            ModelState.AddModelError("Username", "Username already exists.");

        // Only include validation for chosen user type.

        if (viewModel.UserType == UserType.Adopter)
        {
            ModelState.Remove("Name");
            ModelState.Remove("State");
        }
        else if (viewModel.UserType == UserType.Shelter)
        {
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
        }
        else
        {
            ModelState.Remove("Name");
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("State");
        }

        // If registration data is invalid, show errors. 

        if (!ModelState.IsValid)
            return View(nameof(Index), viewModel);

        // If registration data is valid, create account.

        Login login = new()
        {
            Email = viewModel.Email,
            PasswordHash = SimpleHash.Compute(viewModel.Password),
        };

        _loginManager.Add(login);

        if (viewModel.UserType == UserType.Adopter)
        {
            Adopter adopter = new()
            {
                Username = viewModel.Username,
                LoginId = login.Id,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName
            };
            _adopterManager.Add(adopter);
            _loginManager.Update(login);
        }
        else
        {
            Shelter shelter = new()
            {
                Username = viewModel.Username,
                LoginId = login.Id,
                Name = viewModel.Name,
                State = (State)viewModel.State
            };
            _shelterManager.Add(shelter);
            _loginManager.Update(login);
        }

        // Send verification email.

        await _emailService.SendEmailAsync(login.Email, EmailType.Verification, login.VerificationToken);
        TempData["email"] = login.Email;

        return RedirectToAction("Verify", "Email");
    }

    public async Task<IActionResult> ResendVerification(string email)
    {
        Login login = _loginManager.GetByEmail(email);

        // Check if user is verified and return home if they are.

        if (login is null || login.Verified)
            RedirectToAction("Index", "Home");

        // Send verification email.

        await _emailService.SendEmailAsync(login.Email, EmailType.Verification, login.VerificationToken);

        TempData["email"] = login.Email;
        TempData["title"] = "Verification resent";

        return RedirectToAction("Verify", "Email");
    }
}