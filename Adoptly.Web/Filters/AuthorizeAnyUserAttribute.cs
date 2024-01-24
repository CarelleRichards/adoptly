using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adoptly.Web.Filters;

public class AuthorizeAnyUserAttribute : Attribute, IAuthorizationFilter
{
    // Return to homepage is user is not logged.

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string username = context.HttpContext.Session.GetString("Username");

        if (username is null)
            context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}