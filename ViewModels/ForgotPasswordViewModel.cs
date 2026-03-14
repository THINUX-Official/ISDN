using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Only @gmail.com accounts are allowed to reset passwords.")]
        public string Email { get; set; } = string.Empty;
    }
}
