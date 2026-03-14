namespace ISDN.ViewModels
{
    public class CustomerProfileViewModel
    {
        // Allowed fields for customer to change
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? NewPassword { get; set; } // Only filled if user wants a change

        // Added: Current password required to authorize profile updates
        public string? CurrentPassword { get; set; }

        // To hold Head Office admin emails for the footer
        public List<string> HeadOfficeEmails { get; set; } = new();
    }
}
