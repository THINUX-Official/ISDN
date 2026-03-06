using System;
using System.Collections.Generic;

namespace ISDN.Models
{
    public class DashboardViewModel
    {
        public decimal SalesToday { get; set; }
        public decimal SalesMonth { get; set; }
        public int PendingOrdersCount { get; set; }
        public decimal GrowthToday { get; set; }
        public decimal GrowthMonth { get; set; }
        public List<OrderStatusCount> OrdersByStatus { get; set; } = new List<OrderStatusCount>();
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
        public List<LowStockItem> LowStockItems { get; set; } = new List<LowStockItem>();
        public List<RecentOrder> RecentOrders { get; set; } = new List<RecentOrder>();
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public int RdcId { get; set; } = 1;
    }

    public class OrderStatusCount
    {
        public string? Status { get; set; }
        public int Count { get; set; }
    }

    public class TopProduct
    {
        public string? ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class LowStockItem
    {
        public string? ProductName { get; set; }
        public int QtyOnHand { get; set; }
        public int ReorderLevel { get; set; }
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SalesReportViewModel
    {
        public string?  UserName          { get; set; }
        public string?  UserRole          { get; set; }
        public int      RdcId             { get; set; } = 1;
        public DateTime FromDate          { get; set; }
        public DateTime ToDate            { get; set; }
        public decimal  TotalRevenue      { get; set; }
        public int      TotalOrders       { get; set; }
        public int      CompletedOrders   { get; set; }
        public int      PendingOrders     { get; set; }
        public decimal  AverageOrderValue { get; set; }
        public List<RecentOrder> Orders   { get; set; } = new List<RecentOrder>();
    }

    // ── Inventory Report ViewModels ───────────────────────────────────────────

    public class InventoryReportRow
    {
        public string  ProductName       { get; set; } = "";
        public string  Category          { get; set; } = "";
        public int     QuantityAvailable  { get; set; }
        public int     QuantityReserved   { get; set; }
        public int     ReorderLevel       { get; set; }
        public string  Location           { get; set; } = "";
        public int?    RdcId              { get; set; }
        public bool    IsLowStock         { get; set; }
        public DateTime LastUpdated       { get; set; }
    }

    public class InventoryReportViewModel
    {
        public List<InventoryReportRow> Rows          { get; set; } = new();
        public int     TotalItems         { get; set; }
        public int     LowStockCount      { get; set; }
        public int     TotalQtyAvailable  { get; set; }
    }

    // ── Pending Orders Report ViewModels ─────────────────────────────────────

    public class PendingOrderRow
    {
        public int      OrderId          { get; set; }
        public string   OrderNumber      { get; set; } = "";
        public string   CustomerName     { get; set; } = "";
        public DateTime OrderDate        { get; set; }
        public decimal  TotalAmount      { get; set; }
        public string   DeliveryAddress  { get; set; } = "";
        public int      DaysWaiting      { get; set; }
    }

    public class PendingOrdersReportViewModel
    {
        public List<PendingOrderRow> Orders { get; set; } = new();
        public int     TotalPending  { get; set; }
        public decimal TotalValue    { get; set; }
    }
}
