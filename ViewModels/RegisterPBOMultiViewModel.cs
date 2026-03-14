using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    public class RegisterPBOMultiViewModel
    {
        // Owner Info
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business Name is required")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Nested Logic: Multiple Business Types
        // Requirement: At least 2 business types
        public List<BusinessTypeGroupViewModel> BusinessGroups { get; set; } = new();

        public string RegistrationPreference { get; set; } = "Code"; // "Code" or "Self"
    }

    public class BusinessTypeGroupViewModel
    {
        [Required(ErrorMessage = "Business Type is required (e.g. Bakery, Supermarket)")]
        [Display(Name = "Business Type")]
        public string BusinessType { get; set; } = string.Empty;

        

        // Requirement: At least 1 branch per business type
        public List<BranchViewModel> Branches { get; set; } = new();
    }
}
