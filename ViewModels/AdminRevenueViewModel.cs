using System;

namespace ISDN.ViewModels
{
    public class AdminRevenueViewModel
    {
        public decimal TotalSales { get; set; }
        public decimal TotalReturns { get; set; }
        public decimal NetRevenue { get; set; }
        public int CustomerCount { get; set; }
        public int CompletedOrders { get; set; }
        public int? RdcId { get; set; }
        public string RdcName { get; set; } = "All RDCs";
    }
}
