using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Controllers
{
    /// <summary>
    /// RDC Staff Dashboard Controller
    /// Manages inventory and processes orders with RDC-based data partitioning
    /// </summary>
    [Authorize(Roles = UserRoles.RdcStaff)]
    public class RdcStaffController : BaseRdcController
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IsdnDbContext _context;

        public RdcStaffController(IProductRepository productRepository, IOrderRepository orderRepository, IsdnDbContext context)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _context = context;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.RdcId = GetUserRdcId();
            ViewBag.IsHeadOffice = IsHeadOfficeUser();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            // Get inventory with RDC filtering
            var inventoryQuery = _context.Inventories
                .Include(i => i.Product)
                .AsQueryable();

            // Apply RDC filter
            inventoryQuery = ApplyRdcFilter(inventoryQuery);

            var inventory = await inventoryQuery
                .OrderBy(i => i.Product.ProductName)
                .ToListAsync();

            ViewBag.RdcId = GetUserRdcId();
            return View(inventory);
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            // Get orders with RDC filtering
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Apply RDC filter
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.RdcId = GetUserRdcId();
            return View(orders);
        }
    }
}
