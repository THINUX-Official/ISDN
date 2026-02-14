using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    /// <summary>
    /// ViewModel for user login with JWT authentication.
    /// Used to bind form data from the login page and perform validation.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User's email address used as login credential
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// URL to redirect after successful login
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}
