using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Adoptly.Web.Models;

namespace Adoptly.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult AboutUs() => View();

    public IActionResult ContactUs() => View();

    public IActionResult PrivacyPolicy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
