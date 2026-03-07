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
    [Authorize(Roles = "ADMIN,HEAD_OFFICE,FINANCE,SALES_REP")]
    public class ManageDashboardController : Controller  // FIX 1: was DashboardController
    {
        private readonly IsdnDbContext _context;  // FIX 2: was ApplicationDbContext

        public ManageDashboardController(IsdnDbContext context)  // FIX 3: was DashboardController(ApplicationDbContext
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();
            
            model.UserName = User.Identity?.Name ?? "User";
            model.UserRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "Unknown";
            
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            // 1. SALES TODAY
            var today = DateTime.Today;
            var salesTodayQuery = _context.Orders
                .Where(o => o.OrderDate.Date == today 
                    && (o.Status == "DELIVERED" || o.Status == "COMPLETED"));
            
            if (userRdcId.HasValue)
                salesTodayQuery = salesTodayQuery.Where(o => o.RdcId == userRdcId.Value);
            
            model.SalesToday = await salesTodayQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // 2. SALES THIS MONTH
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var salesMonthQuery = _context.Orders
                .Where(o => o.OrderDate >= firstDayOfMonth 
                    && o.OrderDate <= today
                    && (o.Status == "DELIVERED" || o.Status == "COMPLETED"));
            
            if (userRdcId.HasValue)
                salesMonthQuery = salesMonthQuery.Where(o => o.RdcId == userRdcId.Value);
            
            model.SalesMonth = await salesMonthQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // 3. PENDING ORDERS
            var pendingQuery = _context.Orders.Where(o => o.Status == "PENDING");
            
            if (userRdcId.HasValue)
                pendingQuery = pendingQuery.Where(o => o.RdcId == userRdcId.Value);
            
            model.PendingOrdersCount = await pendingQuery.CountAsync();

            // 4. ORDERS BY STATUS (Last 30 days)
            var thirtyDaysAgo = today.AddDays(-30);
            var ordersByStatusQuery = _context.Orders
                .Where(o => o.OrderDate >= thirtyDaysAgo);
            
            if (userRdcId.HasValue)
                ordersByStatusQuery = ordersByStatusQuery.Where(o => o.RdcId == userRdcId.Value);
            
            model.OrdersByStatus = await ordersByStatusQuery
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount
                {
                    Status = g.Key,
                    Count  = g.Count()
                })
                .ToListAsync();

            // 5. TOP PRODUCTS (Last 30 days)
            var topProductsQuery = from oi in _context.OrderItems
                                   join o in _context.Orders   on oi.OrderId   equals o.OrderId
                                   join p in _context.Products on oi.ProductId equals p.ProductId
                                   where o.OrderDate >= thirtyDaysAgo
                                       && (o.Status == "DELIVERED" || o.Status == "COMPLETED")
                                   select new { o.RdcId, p.ProductName, oi.Quantity };
            
            if (userRdcId.HasValue)
                topProductsQuery = topProductsQuery.Where(x => x.RdcId == userRdcId.Value);
            
            model.TopProducts = await topProductsQuery
                .GroupBy(x => x.ProductName)
                .Select(g => new TopProduct
                {
                    ProductName   = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(tp => tp.TotalQuantity)
                .Take(5)
                .ToListAsync();

            // 6. LOW STOCK ITEMS
            // FIX 4: was i.QtyOnHand — correct column is QuantityAvailable (see Inventory.cs)
            var lowStockQuery = from i in _context.Inventories
                                join p in _context.Products on i.ProductId equals p.ProductId
                                where i.QuantityAvailable <= i.ReorderLevel
                                select new { i.RdcId, p.ProductName, i.QuantityAvailable, i.ReorderLevel };
            
            if (userRdcId.HasValue)
                lowStockQuery = lowStockQuery.Where(x => x.RdcId == userRdcId.Value);
            
            model.LowStockItems = await lowStockQuery
                .OrderBy(x => x.QuantityAvailable)
                .Take(10)
                .Select(x => new LowStockItem
                {
                    ProductName  = x.ProductName,
                    QtyOnHand    = x.QuantityAvailable,
                    ReorderLevel = x.ReorderLevel
                })
                .ToListAsync();

            // 7. RECENT ORDERS
            var recentOrdersQuery = _context.Orders.AsQueryable();
            
            if (userRdcId.HasValue)
                recentOrdersQuery = recentOrdersQuery.Where(o => o.RdcId == userRdcId.Value);
            
            model.RecentOrders = await recentOrdersQuery
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrder
                {
                    OrderId     = o.OrderId,
                    OrderDate   = o.OrderDate,
                    Status      = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

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
            var fromDate = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var toDate   = to   ?? DateTime.Today;

            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            var ordersQuery = _context.Orders
                .Where(o => o.OrderDate.Date >= fromDate.Date 
                    && o.OrderDate.Date <= toDate.Date);

            if (userRdcId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.RdcId == userRdcId.Value);

            // FIX 5: was projecting into OrderDetail which doesn't exist — use RecentOrder
            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new RecentOrder
                {
                    OrderId     = o.OrderId,
                    OrderDate   = o.OrderDate,
                    Status      = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            var model = new SalesReportViewModel
            {
                FromDate          = fromDate,
                ToDate            = toDate,
                Orders            = orders,
                TotalOrders       = orders.Count,
                TotalRevenue      = orders.Sum(o => o.TotalAmount),
                CompletedOrders   = orders.Count(o => o.Status == "DELIVERED" || o.Status == "COMPLETED"),
                PendingOrders     = orders.Count(o => o.Status == "PENDING"),
                AverageOrderValue = orders.Count > 0 ? orders.Sum(o => o.TotalAmount) / orders.Count : 0
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSalesReport(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var toDate   = to   ?? DateTime.Today;

            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            var ordersQuery = _context.Orders
                .Where(o => o.OrderDate.Date >= fromDate.Date 
                    && o.OrderDate.Date <= toDate.Date);

            if (userRdcId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.RdcId == userRdcId.Value);

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Order ID,Date,Status,Total Amount");

            foreach (var order in orders)
                csv.AppendLine($"{order.OrderId},{order.OrderDate:yyyy-MM-dd},{order.Status},{order.TotalAmount:F2}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"sales_report_{DateTime.Today:yyyy-MM-dd}.csv");
        }

        // ── Inventory Report ─────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> InventoryReport()
        {
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            var query = from i in _context.Inventories
                        join p in _context.Products on i.ProductId equals p.ProductId
                        select new InventoryReportRow
                        {
                            ProductName       = p.ProductName,
                            Category          = p.Category ?? "—",
                            QuantityAvailable = i.QuantityAvailable,
                            QuantityReserved  = i.QuantityReserved,
                            ReorderLevel      = i.ReorderLevel,
                            Location          = i.Location ?? "—",
                            RdcId             = i.RdcId,
                            IsLowStock        = i.QuantityAvailable <= i.ReorderLevel,
                            LastUpdated       = i.LastUpdated
                        };

            if (userRdcId.HasValue)
                query = query.Where(x => x.RdcId == userRdcId.Value);

            var rows = await query.OrderBy(x => x.ProductName).ToListAsync();

            var model = new InventoryReportViewModel
            {
                Rows         = rows,
                TotalItems   = rows.Count,
                LowStockCount = rows.Count(r => r.IsLowStock),
                TotalQtyAvailable = rows.Sum(r => r.QuantityAvailable)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportInventoryReport()
        {
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            var query = from i in _context.Inventories
                        join p in _context.Products on i.ProductId equals p.ProductId
                        select new { p.ProductName, p.Category, i.QuantityAvailable, i.QuantityReserved, i.ReorderLevel, i.Location, i.RdcId, i.LastUpdated };

            if (userRdcId.HasValue)
                query = query.Where(x => x.RdcId == userRdcId.Value);

            var rows = await query.OrderBy(x => x.ProductName).ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Product,Category,Qty Available,Qty Reserved,Reorder Level,Location,Last Updated");
            foreach (var r in rows)
                csv.AppendLine($"{r.ProductName},{r.Category},{r.QuantityAvailable},{r.QuantityReserved},{r.ReorderLevel},{r.Location},{r.LastUpdated:yyyy-MM-dd}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"inventory_report_{DateTime.Today:yyyy-MM-dd}.csv");
        }

        // ── Pending Orders Report ─────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> PendingOrdersReport()
        {
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            // Use projection to avoid selecting columns that may not exist (e.g. admin_status)
            var baseQuery = _context.Orders
                .Where(o => o.Status == "PENDING");

            if (userRdcId.HasValue)
                baseQuery = baseQuery.Where(o => o.RdcId == userRdcId.Value);

            var orders = await baseQuery
                .OrderBy(o => o.OrderDate)
                .Select(o => new PendingOrderRow
                {
                    OrderId         = o.OrderId,
                    OrderNumber     = o.OrderNumber,
                    CustomerName    = o.User != null ? o.User.FullName : "—",
                    OrderDate       = o.OrderDate,
                    TotalAmount     = o.TotalAmount,
                    DeliveryAddress = o.DeliveryAddress ?? "—",
                    DaysWaiting     = (DateTime.Today - o.OrderDate.Date).Days
                })
                .ToListAsync();

            var model = new PendingOrdersReportViewModel
            {
                Orders       = orders,
                TotalPending = orders.Count,
                TotalValue   = orders.Sum(o => o.TotalAmount)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPendingOrdersReport()
        {
            var rdcId = User.Claims.FirstOrDefault(c => c.Type == "RdcId")?.Value;
            int? userRdcId = string.IsNullOrEmpty(rdcId) ? null : int.Parse(rdcId);

            var baseQuery = _context.Orders
                .Where(o => o.Status == "PENDING");

            if (userRdcId.HasValue)
                baseQuery = baseQuery.Where(o => o.RdcId == userRdcId.Value);

            var orders = await baseQuery
                .OrderBy(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderNumber,
                    CustomerName    = o.User != null ? o.User.FullName : "—",
                    o.OrderDate,
                    o.TotalAmount,
                    DeliveryAddress = o.DeliveryAddress ?? "—"
                })
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Order ID,Order Number,Customer,Order Date,Total Amount,Delivery Address,Days Waiting");
            foreach (var o in orders)
            {
                var days = (DateTime.Today - o.OrderDate.Date).Days;
                csv.AppendLine($"{o.OrderId},{o.OrderNumber},{o.CustomerName},{o.OrderDate:yyyy-MM-dd},{o.TotalAmount:F2},{o.DeliveryAddress},{days}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"pending_orders_{DateTime.Today:yyyy-MM-dd}.csv");
        }
    }
}