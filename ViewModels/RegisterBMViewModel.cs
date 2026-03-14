using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ISDN.ViewModels
{
    public class RegisterBMViewModel
    {
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

        [Required(ErrorMessage = "Password is required"), MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string InvitationCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a branch to register under")]
        public int SelectedBranchId { get; set; }

        public List<BranchInfo> AvailableBranches { get; set; } = new List<BranchInfo>();
    }

    public class BranchInfo
    {
        public int CustomerId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}