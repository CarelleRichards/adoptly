using Adoptly.Web.Managers;
using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Adoptly.Web.ViewComponents;

public class NotificationPillBadgeViewComponent : ViewComponent
{
    private readonly ApplicationManager _applicationManager;

    public NotificationPillBadgeViewComponent(ApplicationManager applicationManager) => _applicationManager = applicationManager;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Retrieve a list of applications for the user (adopter) who is currently logged in.
        
        List<Application> applications = _applicationManager.GetByAdopterUsername(HttpContext.Session.GetString("Username"));
        
        // Return accepted applications that the user has yet to visit.
        
        bool anyAcceptedApplications = applications.Any(application => application.Status == ApplicationStatus.Accepted && application.Visited == false);

        // If there are any, return a pill badge view component.
        
        return anyAcceptedApplications
            ? new HtmlContentViewComponentResult(
                new HtmlString("""
                               <span class="badge bg-notification p-2 ms-2 rounded-circle">
                               <i class="fa fa-bell"></i>
                               </span>
                               """))
            : Content("");
    }
}