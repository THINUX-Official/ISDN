namespace ISDN.Models.ViewModels
{
    public class CustomerClusterViewModel
    {
        public string UniqueCode { get; set; }
        public string BusinessName { get; set; }
        // BusinessType (e.g., Retail/Wholesale) stored at index 1 in business_name
        public string BusinessType { get; set; }
        // UserType (PBOS/PBOM/SBO) stored at index 2 in business_name
        public string UserType { get; set; }
        // Raw markers found in the cluster's business_name values
        public bool ContainsPBOS { get; set; }
        public bool ContainsPBOM { get; set; }
        public bool ContainsSbo { get; set; }
        public string Email { get; set; }
        public List<CustomerBranchViewModel> Branches { get; set; } = new();
    }

    public class CustomerBranchViewModel
    {
        public int CustomerId { get; set; }
        // BusinessType for the branch (stored at index 1 in business_name)
        public string BusinessType { get; set; }
        // UserType for the branch (PBOS/PBOM/SBO) stored at index 2
        public string UserType { get; set; }
        public string BranchName { get; set; }
        public string City { get; set; }
        public string Status { get; set; }
        public bool IsMainBranch { get; set; }
        public int? RdcId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DisapprovedAt { get; set; }
        public string? Email { get; set; }
        public string? StreetAddress { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
