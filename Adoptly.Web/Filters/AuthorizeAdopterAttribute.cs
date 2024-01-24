using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adoptly.Web.Filters;

public class AuthorizeAdopterAttribute : Attribute, IAuthorizationFilter
{
    // Return to homepage is user is not logged in as an adopter.

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string username = context.HttpContext.Session.GetString("Username");

        if (username is null)
            context.Result = new RedirectToActionResult("Index", "Home", null);

        string role = context.HttpContext.Session.GetString("Role");

        if (role is null || role != "Adopter")
            context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}