using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class AllUsersViewModel
{
    public List<Login> Logins { get; set; } = new();

    [Display(Name = "Sort")]
    public UsersSortOrder SortOrder { get; set; } = UsersSortOrder.NewestAccount;
    
    public string UserEmail { get; set; }
}