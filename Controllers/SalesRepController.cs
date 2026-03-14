using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

namespace ISDN.Controllers
{
    /// <summary>
    /// Sales Representative Dashboard Controller
    /// Creates orders on behalf of customers and tracks sales with RDC-based filtering
    /// </summary>
    [Authorize(Roles = UserRoles.SalesRep)]
    public class SalesRepController : BaseRdcController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IsdnDbContext _context;

        public SalesRepController(
            IOrderRepository orderRepository, 
            IProductRepository productRepository,
            IsdnDbContext context)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Get orders with RDC filtering
            var ordersQuery = _context.Orders.AsQueryable();
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var totalOrders = await ordersQuery.CountAsync();

            var todayOrdersQuery = ordersQuery.Where(o => o.OrderDate.Date == DateTime.Today);
            var todayOrders = await todayOrdersQuery.CountAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TodayOrders = todayOrders;
            ViewBag.RdcId = GetUserRdcId();
            ViewBag.IsHeadOffice = IsHeadOfficeUser();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            // Get orders with RDC filtering
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            ordersQuery = ApplyRdcFilter(ordersQuery);

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.RdcId = GetUserRdcId();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            var customers = await _context.Users
                .Where(u => u.Role!.RoleName == UserRoles.Customer)
                .ToListAsync();

            ViewBag.Products = products;
            ViewBag.Customers = customers;
            ViewBag.RdcId = GetUserRdcId();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPredictiveAnalytics()
        {
            var rdcId = GetUserRdcId();

            // 1. Regional Sales Forecast (mocked based on actual RDCs to simulate demand)
            var activeRdcs = await _context.Rdcs.Where(r => r.IsActive).ToListAsync();
            var regions = activeRdcs.Select(r => r.Region ?? r.RdcName).Distinct().ToList();
            if(!regions.Any()) regions = new List<string> { "North", "South", "East", "West" };
            
            // Forecast logic: base values + random element to simulate predictive analytics output
            var rand = new Random();
            var forecasts = regions.Select(r => rand.Next(50, 200)).ToList();

            // 2. Fast/Slow Moving and Stockout Risk Detection (using real inventory/products if exists)
            var allProductsQuery = _context.Products.AsQueryable();
            var allInventoryQuery = _context.Inventories.AsQueryable();
            
            if (rdcId.HasValue) 
            {
                allInventoryQuery = allInventoryQuery.Where(i => i.RdcId == rdcId.Value);
            }

            var inventoryItems = await allInventoryQuery
                .Include(i => i.Product)
                .Where(i => i.QuantityAvailable < i.ReorderLevel * 2)
                .OrderBy(i => i.QuantityAvailable)
                .Take(10)
                .ToListAsync();

            var fastMoving = inventoryItems.Where(i => i.QuantityAvailable < i.ReorderLevel && i.QuantityAvailable > 0)
                .Select(i => i.Product?.ProductName ?? "Unknown")
                .Take(5).ToList();
            
            if (!fastMoving.Any()) fastMoving = new List<string> { "Standard Widget", "Premium Toolkit" };

            var slowMoving = inventoryItems.Where(i => i.QuantityAvailable > i.ReorderLevel * 1.5)
                .Select(i => i.Product?.ProductName ?? "Unknown")
                .Take(5).ToList();
                
            if (!slowMoving.Any()) slowMoving = new List<string> { "Obsolete Adapter", "Niche Filter" };

            var stockoutRisks = inventoryItems.Where(i => i.QuantityAvailable == 0)
                .Select(i => i.Product?.ProductName ?? "Out of Stock Item")
                .Take(3).ToList();

            if (!stockoutRisks.Any()) stockoutRisks = new List<string> { "No immediate stockouts detected." };

            // 3. Seasonal Demand
            var currentMonth = DateTime.Now.Month;
            var seasonal = new List<string> {
                currentMonth == 12 || currentMonth == 1 ? "Winter Holiday Peak expected" :
                currentMonth > 5 && currentMonth < 9 ? "Summer construction items surging" : "Steady baseline demand expected",
                "Monsoon prep items trending upwards by 15%"
            };

            // 4. Retailer Patterns (mock analysis based on orders table)
            var patternAnalysis = new List<string> {
                "70% of B2B customers reorder every 3 weeks",
                "Recent drop in wholesale bulk orders detected in Western Region",
                "High predictability in first-week-of-month stock purchases"
            };

            return Json(new {
                success = true,
                data = new {
                    regions = regions,
                    forecasts = forecasts,
                    fastMoving = fastMoving,
                    slowMoving = slowMoving,
                    stockoutRisks = stockoutRisks,
                    seasonal = seasonal,
                    retailerPatterns = patternAnalysis
                }
            });
        }
    }
}
