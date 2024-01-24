using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models.Enums;

public enum SortOrder
{
    [Display(Name = "Date listed (newest)")]
    Newest = 1,

    [Display(Name = "Date listed (oldest)")]
    Oldest = 2
}