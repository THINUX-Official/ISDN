using ISDN.Models;
using System.Collections.Generic;

namespace ISDN.ViewModels
{
    /// <summary>
    /// ViewModel for RDC Staff Inventory Management
    /// Shows stock levels, reservations, and quarantine items
    /// </summary>
    public class InventoryViewModel
    {
        public List<InventoryItemViewModel> NormalStock { get; set; } = new List<InventoryItemViewModel>();
        public List<InventoryItemViewModel> QuarantineStock { get; set; } = new List<InventoryItemViewModel>();
        public int RdcId { get; set; }
        public string RdcName { get; set; } = string.Empty;
    }

    public class InventoryItemViewModel
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public int ReorderLevel { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }

        // Calculated properties
        public int TotalStock => QuantityAvailable + QuantityReserved;
        public int FreeStock => QuantityAvailable - QuantityReserved;
        public bool IsLowStock => QuantityAvailable <= ReorderLevel;
        public bool IsCriticalStock => QuantityAvailable < (ReorderLevel / 2);
        public bool IsOutOfStock => QuantityAvailable == 0;
        public bool IsQuarantine => Location == "RETURNS-HOLD";
        
        public string StockStatusClass
        {
            get
            {
                if (IsOutOfStock) return "danger";
                if (IsCriticalStock) return "danger";
                if (IsLowStock) return "warning";
                return "success";
            }
        }

        public string StockStatusText
        {
            get
            {
                if (IsOutOfStock) return "Out of Stock";
                if (IsCriticalStock) return "Critical";
                if (IsLowStock) return "Low Stock";
                return "In Stock";
            }
        }
    }
}
