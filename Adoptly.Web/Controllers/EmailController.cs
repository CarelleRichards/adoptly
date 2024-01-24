using Adoptly.Web.Filters;
using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Adoptly.Web.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;

namespace Adoptly.Web.Controllers;

public class EmailController : Controller
{
    private readonly LoginManager _loginManager;
    private readonly EmailService _emailService;
    private static readonly ISimpleHash SimpleHash = new SimpleHash();

    public EmailController(LoginManager loginManager, EmailService emailService)
    {
        _loginManager = loginManager;
        _emailService = emailService;
    }

    public IActionResult Verified()
    {
        if(HttpContext.Session.IsAvailable)
            HttpContext.Session.Clear();
        return View();
    }

    public IActionResult Verify()
    {
        ViewBag.Email = TempData["email"].ToString();
        ViewBag.Title = TempData.ContainsKey("title") ? TempData["title"].ToString() : "Verify your email";
        return View();
    }

    public IActionResult ResetPassword() => View(new ResetPasswordViewModel());

    public IActionResult ChangePassword(string email) => View(new ChangePasswordViewModel { Email = email });

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(nameof(ResetPassword), viewModel);

        // Get login by email.
        
        var login = _loginManager.GetByEmail(viewModel.Email);

        // Check if login is null, and return the view.
        
        if (login is null)
        {
            ModelState.AddModelError("Failure", "If you have an account, we'll send you an email.");
            return View(nameof(ResetPassword), viewModel);
        }

        // Send reset password email.
        
        await _emailService.SendEmailAsync(login.Email, EmailType.ResetPassword, login.VerificationToken);
        ViewBag.Success = "If you have an account, we'll send you an email.";
        return View(nameof(ResetPassword), viewModel);
    }

    [HttpPost]
    [AuthorizeTempUser]
    public IActionResult ChangePassword(ChangePasswordViewModel viewModel)
    {
        // Check if authorised user is accessing the view.
        if (HttpContext.Session.GetString("TempEmail") != viewModel.Email)
            return RedirectToAction(nameof(Index), "Home");
        
        if (!ModelState.IsValid)
            return View(nameof(ChangePassword), viewModel);

        // Get login by email.
        
        Login login = _loginManager.GetByEmail(viewModel.Email);

        if (login is null)
            return RedirectToAction(nameof(Index), "Home");

        // Set the new password.
        
        login.PasswordHash = SimpleHash.Compute(viewModel.Password);
        login.VerificationToken = Guid.NewGuid().ToString();
        _loginManager.Update(login);
        
        // Clear the temporary session data.
        
        HttpContext.Session.Clear();

        // Return to the login page.
        
        return login.User is Admin
            ? RedirectToAction(nameof(Admin), "Login")
            : RedirectToAction(nameof(Index), "Login");
    }
}