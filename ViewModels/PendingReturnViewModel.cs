using System.Collections.Generic;

namespace ISDN.ViewModels
{
    public class PendingReturnItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class PendingReturnViewModel
    {
        public int ReturnId { get; set; }
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string? AdminComment { get; set; }
        public string Status { get; set; } = "Pending";

        // Customer info
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }

        // Items associated with the return
        public List<PendingReturnItemViewModel> Items { get; set; } = new List<PendingReturnItemViewModel>();
    }
}
