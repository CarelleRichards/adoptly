using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models.Enums;

public enum UsersSortOrder
{
    [Display(Name = "Account created (newest)")]
    NewestAccount = 1,

    [Display(Name = "Account created (oldest)")]
    OldestAccount = 2
}