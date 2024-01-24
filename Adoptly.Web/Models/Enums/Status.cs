using System.ComponentModel.DataAnnotations;

namespace Adoptly.Web.Models.Enums;

public enum Status
{
    Available = 1,

    [Display(Name = "On hold")]
    OnHold = 2,

    Adopted = 3,

    Unavailable = 4
}