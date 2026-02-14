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
    }
}
