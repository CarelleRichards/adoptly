using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adoptly.Web.Filters;

public class AuthorizeShelterAttribute : Attribute, IAuthorizationFilter
{
    // Return to homepage is user is not logged in as a shelter.

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string username = context.HttpContext.Session.GetString("Username");

        if (username is null)
            context.Result = new RedirectToActionResult("Index", "Home", null);

        string role = context.HttpContext.Session.GetString("Role");

        if (role is null || role != "Shelter")
            context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}