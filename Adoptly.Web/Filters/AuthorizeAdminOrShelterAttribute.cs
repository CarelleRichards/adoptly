using Adoptly.Web.Models;

namespace Adoptly.Web.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorizeAdminOrShelterAttribute : Attribute, IAuthorizationFilter
{
    // Return to homepage is user is not logged in as an admin or a shelter.

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string username = context.HttpContext.Session.GetString("Username");

        if (username is null)
            context.Result = new RedirectToActionResult("Index", "Home", null);

        string role = context.HttpContext.Session.GetString("Role");

        if (role is not (nameof(Admin) or nameof(Shelter)))
            context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}