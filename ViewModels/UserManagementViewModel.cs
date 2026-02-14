namespace ISDN.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing user information in the Admin's user management panel.
    /// Contains user details and role information for administrative purposes.
    /// </summary>
    public class UserManagementViewModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Display name of the user
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Current role assigned to the user
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Role ID
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Whether the user account is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When the user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Whether two-factor authentication is enabled
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}
