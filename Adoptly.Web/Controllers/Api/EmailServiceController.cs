using Adoptly.Web.Filters;
using Adoptly.Web.Models;
using Adoptly.Web.Managers;
using Microsoft.AspNetCore.Mvc;

namespace Adoptly.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class EmailServiceController : ControllerBase
{
    private readonly LoginManager _loginManager;

    public EmailServiceController(LoginManager loginManager) => _loginManager = loginManager;

    // Verify user email address.
    
    [HttpGet("VerifyEmail/{email}/{verificationToken}")]
    public IActionResult VerifyUserEmailAsync(string email, string verificationToken)
    {
        // Get login by email.
        
        Login login = _loginManager.GetByEmail(email);

        // Check if the login is null or the verification token is invalid, and return to the home page.
        
        if (login is null || login.VerificationToken != verificationToken)
            return RedirectToAction("Index", "Home");

        // Verify user email.
        
        login.Verified = true;
        login.VerificationToken = Guid.NewGuid().ToString();
        _loginManager.Update(login);

        return RedirectToAction("Verified", "Email");
    }

    // Change user email address.
    
    [AuthorizeAnyUser]
    [HttpGet("ChangeEmail/{email}/{oldEmail}/{verificationToken}")]
    public IActionResult ChangeUserEmailAsync(string email, string oldEmail, string verificationToken)
    {
        // Get login by email.
        
        Login login = _loginManager.GetByEmail(oldEmail);
        
        // Check if the authorised user is accessing the end-point.
        
        if (_loginManager.GetByUsername(HttpContext.Session.GetString("Username")).Email != oldEmail)
            return RedirectToAction(nameof(Index), "Home");

        // Check if the login is null or the verification token is invalid, and return to the home page.

        if (login is null || login.VerificationToken != verificationToken)
            return RedirectToAction("Index", "Home");

        // Change email.
        
        login.Email = email;
        login.VerificationToken = Guid.NewGuid().ToString();
        _loginManager.Update(login);

        return RedirectToAction("Verified", "Email");
    }

    // Reset user password.
    
    [HttpGet("ResetPassword/{email}/{verificationToken}")]
    public IActionResult ResetUserPasswordAsync(string email, string verificationToken)
    {
        // Get login by email.
        
        Login login = _loginManager.GetByEmail(email);

        // Check if the login is null or the verification token is invalid, and return to the home page.

        if (login is null || login.VerificationToken != verificationToken)
            return RedirectToAction("Index", "Home");

        // Set temporary session data and redirect to the change password view.
        
        HttpContext.Session.SetString("TempEmail", login.Email);

        return RedirectToAction("ChangePassword", "Email",new { email = login.Email });
    }
}