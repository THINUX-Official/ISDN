using System;
using System.Collections.Generic;

namespace ISDN.Models
{
    public class DashboardViewModel
    {
        // Summary Stats
        public decimal SalesToday { get; set; }
        public decimal SalesMonth { get; set; }
        public int PendingOrdersCount { get; set; }
        public decimal GrowthToday { get; set; }
        public decimal GrowthMonth { get; set; }
        
        // Orders by Status
        public List<OrderStatusCount> OrdersByStatus { get; set; } = new List<OrderStatusCount>();
        
        // Top Products
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
        
        // Low Stock Items
        public List<LowStockItem> LowStockItems { get; set; } = new List<LowStockItem>();
        
        // Recent Orders
        public List<RecentOrder> RecentOrders { get; set; } = new List<RecentOrder>();
        
        // User Info
        public string UserName { get; set; }
        public string UserRole { get; set; }
    }
    
    public class OrderStatusCount
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }
    
    public class TopProduct
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }
    
    public class LowStockItem
    {
        public string ProductName { get; set; }
        public int QtyOnHand { get; set; }
        public int ReorderLevel { get; set; }
    }
    
    public class RecentOrder
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
