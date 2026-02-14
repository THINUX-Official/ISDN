using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    public class RegisterViewModel
    {
        // --- Personal Information ---
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required"), Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // --- Billing Address ---
        [Required(ErrorMessage = "Business name is required")]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required")]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        // --- Account Security ---
        [Required(ErrorMessage = "Password is required"), MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Hidden field for default status
        public string RegistrationStatus { get; set; } = "PENDING";
    

/// <summary>
/// Role to assign to the user (defaults to CUSTOMER)
/// </summary>
public string RoleName { get; set; } = "CUSTOMER";
    }
}
