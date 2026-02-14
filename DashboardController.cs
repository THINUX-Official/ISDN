using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ISDN.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();
            
            // Get current user info from session/claims
            model.UserName = User.Identity.Name ?? "User";
            model.UserRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "Unknown";
            
            // Get user's RDC ID if applicable (for filtering)
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            // 1. SALES TODAY
            var today = DateTime.Today;
            var salesTodayQuery = _context.Orders
                .Where(o => o.OrderDate.Date == today 
                    && (o.Status == "DELIVERED" || o.Status == "COMPLETED"));
            
            if (userRdcId.HasValue)
            {
                salesTodayQuery = salesTodayQuery.Where(o => o.RdcId == userRdcId.Value);
            }
            
            model.SalesToday = await salesTodayQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // 2. SALES THIS MONTH
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var salesMonthQuery = _context.Orders
                .Where(o => o.OrderDate >= firstDayOfMonth 
                    && o.OrderDate <= today
                    && (o.Status == "DELIVERED" || o.Status == "COMPLETED"));
            
            if (userRdcId.HasValue)
            {
                salesMonthQuery = salesMonthQuery.Where(o => o.RdcId == userRdcId.Value);
            }
            
            model.SalesMonth = await salesMonthQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // 3. PENDING ORDERS
            var pendingQuery = _context.Orders.Where(o => o.Status == "PENDING");
            
            if (userRdcId.HasValue)
            {
                pendingQuery = pendingQuery.Where(o => o.RdcId == userRdcId.Value);
            }
            
            model.PendingOrdersCount = await pendingQuery.CountAsync();

            // 4. ORDERS BY STATUS (Last 30 days)
            var thirtyDaysAgo = today.AddDays(-30);
            var ordersByStatusQuery = _context.Orders
                .Where(o => o.OrderDate >= thirtyDaysAgo);
            
            if (userRdcId.HasValue)
            {
                ordersByStatusQuery = ordersByStatusQuery.Where(o => o.RdcId == userRdcId.Value);
            }
            
            model.OrdersByStatus = await ordersByStatusQuery
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // 5. TOP PRODUCTS (Last 30 days)
            var topProductsQuery = from oi in _context.OrderItems
                                   join o in _context.Orders on oi.OrderId equals o.OrderId
                                   join p in _context.Products on oi.ProductId equals p.ProductId
                                   where o.OrderDate >= thirtyDaysAgo
                                       && (o.Status == "DELIVERED" || o.Status == "COMPLETED")
                                   select new { o.RdcId, p.ProductName, oi.Quantity };
            
            if (userRdcId.HasValue)
            {
                topProductsQuery = topProductsQuery.Where(x => x.RdcId == userRdcId.Value);
            }
            
            model.TopProducts = await topProductsQuery
                .GroupBy(x => x.ProductName)
                .Select(g => new TopProduct
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(tp => tp.TotalQuantity)
                .Take(5)
                .ToListAsync();

            // 6. LOW STOCK ITEMS
            var lowStockQuery = from i in _context.Inventories
                                join p in _context.Products on i.ProductId equals p.ProductId
                                where i.QtyOnHand <= i.ReorderLevel
                                select new { i.RdcId, p.ProductName, i.QtyOnHand, i.ReorderLevel };
            
            if (userRdcId.HasValue)
            {
                lowStockQuery = lowStockQuery.Where(x => x.RdcId == userRdcId.Value);
            }
            
            model.LowStockItems = await lowStockQuery
                .OrderBy(x => x.QtyOnHand)
                .Take(10)
                .Select(x => new LowStockItem
                {
                    ProductName = x.ProductName,
                    QtyOnHand = x.QtyOnHand,
                    ReorderLevel = x.ReorderLevel
                })
                .ToListAsync();

            // 7. RECENT ORDERS
            var recentOrdersQuery = _context.Orders.AsQueryable();
            
            if (userRdcId.HasValue)
            {
                recentOrdersQuery = recentOrdersQuery.Where(o => o.RdcId == userRdcId.Value);
            }
            
            model.RecentOrders = await recentOrdersQuery
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrder
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            // Mock growth percentages (you can calculate real values later)
            model.GrowthToday = 12.5m;
            model.GrowthMonth = 8.3m;

            return View(model);
        }

        public IActionResult Report()
        {
            return RedirectToAction("SalesReport");
        }

        public async Task<IActionResult> SalesReport(DateTime? from, DateTime? to)
        {
            var model = new SalesReportViewModel
            {
                FromDate = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                ToDate = to ?? DateTime.Today
            };

            // Get user's RDC ID if applicable
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            // Get orders in date range
            var ordersQuery = _context.Orders
                .Where(o => o.OrderDate.Date >= model.FromDate.Date 
                    && o.OrderDate.Date <= model.ToDate.Date);

            if (userRdcId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.RdcId == userRdcId.Value);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDetail
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            model.Orders = orders;
            model.TotalOrders = orders.Count;
            model.TotalRevenue = orders.Sum(o => o.TotalAmount);
            model.CompletedOrders = orders.Count(o => o.Status == "DELIVERED" || o.Status == "COMPLETED");
            model.PendingOrders = orders.Count(o => o.Status == "PENDING");
            model.AverageOrderValue = model.TotalOrders > 0 ? model.TotalRevenue / model.TotalOrders : 0;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSalesReport(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var toDate = to ?? DateTime.Today;

            // Get user's RDC ID if applicable
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            var ordersQuery = _context.Orders
                .Where(o => o.OrderDate.Date >= fromDate.Date 
                    && o.OrderDate.Date <= toDate.Date);

            if (userRdcId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.RdcId == userRdcId.Value);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Generate CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Order ID,Date,Status,Total Amount");

            foreach (var order in orders)
            {
                csv.AppendLine($"{order.OrderId},{order.OrderDate:yyyy-MM-dd},{order.Status},{order.TotalAmount:F2}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"sales_report_{DateTime.Today:yyyy-MM-dd}.csv");
        }
    }
}
