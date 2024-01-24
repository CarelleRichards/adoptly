using Microsoft.AspNetCore.Mvc;

namespace Adoptly.Web.Controllers;

public class ErrorController : Controller
{
    public IActionResult PageNotFound() => View();
}