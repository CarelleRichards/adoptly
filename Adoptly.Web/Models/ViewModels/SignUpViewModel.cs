using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Adoptly.Web.Attributes;
using Adoptly.Web.Models.Enums;

namespace Adoptly.Web.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "You must select an account type.")]
        [DisplayName("Account type")]
        public UserType? UserType { get; set; }

        [RequiredNotEmpty(ErrorMessage = "You must enter your first name.")]
        [DisplayName("First name")]
        [StringLength(30, ErrorMessage = "Cannot be more than 30 characters.")]
        public string FirstName { get; set; }

        [RequiredNotEmpty(ErrorMessage = "You must enter your last name.")]
        [DisplayName("Last name")]
        [StringLength(30, ErrorMessage = "Cannot be more than 30 characters.")]
        public string LastName { get; set; }

        [EnumDataType(typeof(State))]
        [Required(ErrorMessage = "You must select a state.")]
        public State? State { get; set; }
        
        [RequiredNotEmpty(ErrorMessage = "You must enter a name.")]
        [StringLength(30, ErrorMessage = "Cannot be more than 30 characters.")]
        public string Name { get; set; }

        [RequiredNotEmpty(ErrorMessage = "You must enter your email.")]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        [StringLength(320, ErrorMessage = "Cannot be more than 320 characters.")]
        public string Email { get; set; }

        [RequiredNotEmpty(ErrorMessage = "You must enter a username.")]
        [StringLength(30, ErrorMessage = "Cannot be more than 30 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9]+([_ -]?[a-zA-Z0-9]){2,}$", ErrorMessage = "Username must have at least three characters, begin and end with a letter or number and can only contain some special special characters, including hyphens and underscores.")]
        public string Username { get; set; }

        [RequiredNotEmpty(ErrorMessage = "You must enter a password.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must have at least eight characters, including an uppercase letter, lowercase letter, number and special character.")]
        public string Password { get; set; }
    }
}