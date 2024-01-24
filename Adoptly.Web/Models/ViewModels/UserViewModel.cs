using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models;

public class UserViewModel
{
    public string Username { get; set; }

    public string ProfilePicture { get; set; }

    public DateTime? DateVerified { get; set; }

    public UserType UserType { get; set; }
}