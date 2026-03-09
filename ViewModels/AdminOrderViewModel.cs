using ISDN.Models;
using System.Collections.Generic;
using System.Linq;

namespace ISDN_Distribution.Models
{
    public class AdminOrderViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderReturn> ActiveReturns { get; set; } = new List<OrderReturn>();

        public bool IsStockAvailable { get; set; } = true;
        public bool IsPacked { get; set; }

        // Updated status matching database uppercase strings
        public bool IsPendingConfirmation => Order.Status == "PLACED" && !ActiveReturns.Any();
        public bool IsPackedAndActive => Order.Status == "PACKED" && !ActiveReturns.Any();
        public bool HasPendingReturn => ActiveReturns.Any();
    }
}