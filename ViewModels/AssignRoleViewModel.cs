using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    /// <summary>
    /// ViewModel for assigning roles to users by administrators.
    /// Provides a structured way to update user role assignments.
    /// </summary>
    public class AssignRoleViewModel
    {
        /// <summary>
        /// The user ID to assign roles to
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User's full name for display
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's email for confirmation
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Current role name
        /// </summary>
        public string CurrentRole { get; set; } = string.Empty;

        /// <summary>
        /// The role to be assigned to the user
        /// </summary>
        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;

        /// <summary>
        /// Available roles in the system (for dropdown)
        /// </summary>
        public List<string> AvailableRoles { get; set; } = new List<string> 
        { 
            "ADMIN", 
            "HEAD_OFFICE", 
            "RDC_STAFF", 
            "LOGISTICS", 
            "DRIVER", 
            "FINANCE", 
            "SALES_REP", 
            "CUSTOMER" 
        };
    }
}
