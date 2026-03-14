namespace ISDN.Models.ViewModels
{
    public class CustomerClusterViewModel
    {
        public string UniqueCode { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string Email { get; set; }
        public List<CustomerBranchViewModel> Branches { get; set; } = new();
    }

    public class CustomerBranchViewModel
    {
        public int CustomerId { get; set; }
        public string BranchName { get; set; }
        public string City { get; set; }
        public string Status { get; set; }
        public bool IsMainBranch { get; set; }
    }
}
