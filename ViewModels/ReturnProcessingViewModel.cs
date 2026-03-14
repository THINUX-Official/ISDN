using ISDN.Models;
using System.Collections.Generic;

namespace ISDN.ViewModels
{
    public class ReturnProcessingViewModel
    {
        public int ReturnId { get; set; }
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string? AdminComment { get; set; }
        public string? AdminStatus { get; set; }
        public string? RefundStatus { get; set; }

        // Computed totals per return (useful when grouping multiple items)
        public decimal TotalRefundAmount { get; set; }
    }
}
