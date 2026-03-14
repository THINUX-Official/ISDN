using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISDN.ViewModels
{
    public class CustomerManagementViewModel
    {
        public List<CustomerClusterDto> PbósCustomers { get; set; } = new List<CustomerClusterDto>();
        public List<CustomerClusterDto> PbómCustomers { get; set; } = new List<CustomerClusterDto>();
        public List<SelectListItem> AvailableRdcs { get; set; } = new List<SelectListItem>();
    }

    public class CustomerClusterDto
    {
        public int CustomerId { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string UserType { get; set; }      // PBOS or PBOM
        public string BusinessType { get; set; }  // e.g., Pharmacy
        public string BusinessName { get; set; }  // e.g., XYZ Corp
        public string BranchName { get; set; }    // e.g., Colombo Branch

        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }

        public int? RdcId { get; set; }
        public string RdcName { get; set; }
        
        public string RegistrationCode { get; set; }
    }
}