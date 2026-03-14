using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ISDN.ViewModels
{
    public class RegisterPBOSingleViewModel : IValidatableObject
    {
        // --- Personal Information ---
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required"), EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business name is required")]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business type is required")]
        [Display(Name = "Business Type")]
        public string BusinessType { get; set; } = string.Empty;

        // --- Multi-Branch Data ---
        public List<BranchViewModel> Branches { get; set; } = new List<BranchViewModel>();

        // Custom validation to ensure at least 2 branches are provided
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Branches == null || Branches.Count < 2)
            {
                yield return new ValidationResult("You must register at least two branches.", new[] { nameof(Branches) });
            }
        }

        // --- Registration Preference ---
        [Required(ErrorMessage = "Please select a registration preference")]
        public string RegistrationPreference { get; set; } = "Code";

        // --- Account Security ---
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class BranchViewModel
    {
        [Required(ErrorMessage = "Branch name is required")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required")]
        public string ZipCode { get; set; } = string.Empty;

        
    }
}
