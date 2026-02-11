using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    /// <summary>
    /// ViewModel for user registration with role selection.
    /// Captures new user information and validates input before creating an account.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// User's full name (e.g., "John Smith")
        /// </summary>
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address - must be unique in the system
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password - must meet security requirements (BCrypt will hash it)
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Password confirmation - must match Password field
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Role to assign to the user (defaults to CUSTOMER)
        /// </summary>
        public string RoleName { get; set; } = "CUSTOMER";
    }
}
