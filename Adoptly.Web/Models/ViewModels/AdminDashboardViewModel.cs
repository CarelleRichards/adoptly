using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models;

public class AdminDashboardViewModel
{
    public AdminReportViewModel AdminReport { get; set; }

    // This is only includes users that are verified.
    [Display(Name = "Newest users")]
    public List<UserViewModel> NewestUsers { get; set; } = new();
}