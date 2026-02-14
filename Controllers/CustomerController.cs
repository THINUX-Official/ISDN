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
    /// CustomerController handles customer operations with proper RDC assignment.
    /// Customers can browse products, place orders (automatically assigned to their RDC),
    /// track deliveries, and view invoices.
    /// </summary>
    [Authorize(Roles = UserRoles.Customer)]
    public class CustomerController : BaseRdcController
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IsdnDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IsdnDbContext context,
            ILogger<CustomerController> logger)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Customer/Dashboard
        /// Customer dashboard with order summary and recent activities
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var customer = await _customerRepository.GetByUserIdAsync(userId);

            // මෙතන තමයි වැරැද්ද තිබුණේ: myOrders කියන්නේ ViewModel එකක්.
            var myOrdersViewModel = await _orderRepository.GetByUserIdAsync(userId);

            // .Orders කියන එක අනිවාර්යයෙන්ම දාන්න ඕනේ Count/Take කරන්න නම්
            ViewBag.TotalOrders = myOrdersViewModel.Orders.Count();
            ViewBag.PendingOrders = myOrdersViewModel.Orders.Count(o => o.Status == "Pending");
            ViewBag.RecentOrders = myOrdersViewModel.Orders.Take(5).ToList();

            ViewBag.CustomerRdcId = customer?.RdcId;
            ViewBag.CustomerRdcName = customer?.Rdc?.RdcName;

            _logger.LogInformation($"Customer dashboard accessed by {User.Identity?.Name}");
            return View();
        }

        /// <summary>
        /// GET: /Customer/Products
        /// Browse available products
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return View(products);
        }

        /// <summary>
        /// GET: /Customer/Orders
        /// View customer's order history
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return View(orders);
        }

        /// <summary>
        /// GET: /Customer/Deliveries
        /// Track delivery status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Deliveries()
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized();
            }

            var deliveries = await _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.Driver)
                .Where(d => d.Order!.UserId == customerId)
                .OrderByDescending(d => d.ScheduledDate)
                .ToListAsync();

            return View(deliveries);
        }

        /// <summary>
        /// GET: /Customer/Invoices
        /// View payment invoices
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized();
            }

            var orders = await _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}

