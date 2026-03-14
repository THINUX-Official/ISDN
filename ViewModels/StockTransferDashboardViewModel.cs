using ISDN.Models;
using System.Collections.Generic;

namespace ISDN.ViewModels
{
    public class StockTransferDashboardViewModel
    {
        public List<StockTransfer> PendingTransfers { get; set; } = new List<StockTransfer>();
        public List<StockTransfer> OutgoingTransfers { get; set; } = new List<StockTransfer>();
        public List<StockTransfer> IncomingTransfers { get; set; } = new List<StockTransfer>();
        public List<StockTransfer> CompletedTransfers { get; set; } = new List<StockTransfer>();
        public List<Inventory> LowStockInventory { get; set; } = new List<Inventory>();
        public List<Rdc> DeactivatedRdcs { get; set; } = new List<Rdc>();
        public List<Rdc> ActiveRdcs { get; set; } = new List<Rdc>();
        
        // Sum of all items in transit
        public int TotalInTransitQuantity { get; set; }
        public int LowStockAlertsCount { get; set; }
    }
}