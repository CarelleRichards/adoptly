using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adoptly.Web.Filters;

public class AuthorizeTempUserAttribute: Attribute, IAuthorizationFilter
{
    // Return to homepage is user is not logged in as a shelter.

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string email = context.HttpContext.Session.GetString("TempEmail");

        if (email is null)
            context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}