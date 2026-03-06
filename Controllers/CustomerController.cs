using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Data;
using ISDN.Models;
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

        // --- CART FUNCTIONALITY ---

        private const string CartSessionKey = "CustomerCart";

        private List<CartItemViewModel> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(sessionData))
            {
                return new List<CartItemViewModel>();
            }
            return System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData) ?? new List<CartItemViewModel>();
        }

        private void SaveCartItems(List<CartItemViewModel> cart)
        {
            var sessionData = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, sessionData);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            if (quantity <= 0) quantity = 1;

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
            {
                TempData["ErrorMessage"] = "Product is unavailable.";
                return RedirectToAction(nameof(Products));
            }

            var cart = GetCartItems();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    UnitPrice = product.UnitPrice,
                    Quantity = quantity,
                    ProductImageUrl = product.ProductImageUrl
                });
            }

            SaveCartItems(cart);
            TempData["SuccessMessage"] = $"{product.ProductName} added to your cart!";
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartItems();
            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartItems(cart);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutCart()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            var userId = GetUserId();
            var customer = await _customerRepository.GetByUserIdAsync(userId);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer profile not found. Cannot place order.";
                return RedirectToAction(nameof(Cart));
            }

            var newOrder = new ISDN.Models.Order
            {
                UserId = userId,
                CustomerId = customer.CustomerId,
                RdcId = customer.RdcId,
                OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(100, 999),
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(c => c.Total),
                Status = "Pending",
                DeliveryAddress = customer.street_address + ", " + customer.city,
                OrderItems = cart.Select(c => new ISDN.Models.OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Subtotal = c.Total
                }).ToList()
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            // Clear the cart
            HttpContext.Session.Remove(CartSessionKey);

            TempData["SuccessMessage"] = $"Order placed successfully! Your order number is {newOrder.OrderNumber}.";
            return RedirectToAction(nameof(Orders));
        }
    }
}

