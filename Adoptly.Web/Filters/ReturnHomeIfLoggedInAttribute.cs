using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adoptly.Web.Filters;

public class ReturnHomeIfLoggedInAttribute : Attribute, IAuthorizationFilter
{
    // Return to homepage if any user is already logged in.

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string username = context.HttpContext.Session.GetString("Username");

        if (username is not null)
            context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}