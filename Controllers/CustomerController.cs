using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Data;
using ISDN.Models;
using Microsoft.EntityFrameworkCore;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.Customer)]
    public class CustomerController : BaseRdcController
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IsdnDbContext _context;
        private readonly ILogger<CustomerController> _logger;
        private readonly IConfiguration _configuration;

        public CustomerController(
            IConfiguration configuration,
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
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> CustomerPaymentHistory()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null) return View(new List<ISDN.Models.Payment>());

            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order != null && p.Order.CustomerId == customer.CustomerId)
                .OrderByDescending(p => p.PaymentDate ?? p.CreatedAt)
                .ToListAsync();

            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            var myOrdersViewModel = await _orderRepository.GetByUserIdAsync(userId);

            ViewBag.TotalOrders = myOrdersViewModel.Orders.Count();
            ViewBag.PendingOrders = myOrdersViewModel.Orders.Count(o => o.Status == "Pending");
            ViewBag.RecentOrders = myOrdersViewModel.Orders.Take(5).ToList();
            ViewBag.CustomerRdcId = customer?.RdcId;
            ViewBag.CustomerRdcName = customer?.Rdc?.RdcName;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Deliveries()
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId)) return Unauthorized();

            var deliveries = await _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.Driver)
                .Where(d => d.Order!.UserId == customerId)
                .OrderByDescending(d => d.ScheduledDate)
                .ToListAsync();

            return View(deliveries);
        }

        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId)) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
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
            return string.IsNullOrEmpty(sessionData) ? new List<CartItemViewModel>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData) ?? new List<CartItemViewModel>();
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
            if (product == null || !product.IsActive) return RedirectToAction(nameof(Products));

            var cart = GetCartItems();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existingItem != null) existingItem.Quantity += quantity;
            else cart.Add(new CartItemViewModel { ProductId = product.ProductId, ProductName = product.ProductName, UnitPrice = product.UnitPrice, Quantity = quantity, ProductImageUrl = product.ProductImageUrl });

            SaveCartItems(cart);
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public IActionResult Cart() => View(GetCartItems());

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null) { cart.Remove(item); SaveCartItems(cart); }
            return RedirectToAction(nameof(Cart));
        }

        // --- NEW PAYMENT FLOW ---

        [HttpGet]
        public async Task<IActionResult> Payment()
        {
            var userId = GetUserId();
            if (userId != 0)
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if (customer != null)
                {
                    ViewBag.FirstName = customer.first_name;
                    ViewBag.LastName = customer.last_name;
                    ViewBag.Address = (customer.street_address + ", " + customer.city).Trim(new char[] { ' ', ',' });
                    ViewBag.ZipCode = customer.zip_code;
                    ViewBag.PhoneNumber = customer.phone_number;
                }
            }

            var items = GetCartItems();
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            return View("~/Views/Payment/Index.cshtml", items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string card_name, string card_number, string exp_month, string exp_year, string cvc, decimal amount, string payment_method, string bank_ref, string CustomerEmail)
        {
            var items = GetCartItems();
            if (!items.Any()) return RedirectToAction(nameof(Cart));

            var userId = GetUserId();
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {
                // try to create a minimal customer record so payment can proceed
                try
                {
                    var username = User?.Identity?.Name ?? ($"user_{userId}");
                    var first = string.Empty;
                    var last = string.Empty;
                    if (!string.IsNullOrEmpty(username))
                    {
                        var parts = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0) first = parts[0];
                        if (parts.Length > 1) last = string.Join(' ', parts.Skip(1));
                    }

                    var newCustomer = new ISDN.Models.Customer
                    {
                        UserId = userId,
                        first_name = string.IsNullOrEmpty(first) ? "Customer" : first,
                        last_name = last ?? string.Empty,
                        email = User?.Identity?.Name,
                        street_address = string.Empty,
                        city = string.Empty,
                        IsActive = true
                    };

                    await _context.Customers.AddAsync(newCustomer);
                    await _context.SaveChangesAsync();
                    customer = newCustomer;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create placeholder customer for user {UserId}", userId);
                    TempData["ErrorMessage"] = "Customer profile not found and could not be created. Please contact support.";
                    return View("~/Views/Payment/Index.cshtml", items);
                }
            }

            // Create Order and Payment inside try/catch so errors are shown on the same page
            var newOrder = new ISDN.Models.Order
            {
                UserId = userId,
                CustomerId = customer.CustomerId,
                RdcId = customer.RdcId,
                // OrderNumber will be generated after saving so we can include a sequence based on OrderId
                OrderNumber = "",
                OrderDate = DateTime.Now,
                TotalAmount = items.Sum(i => i.Total),
                // Orders are created in PLACED state; payment completed separately
                Status = "PLACED",
                DeliveryAddress = (customer.street_address + ", " + customer.city).Trim(new char[] { ' ', ',' }),
                OrderItems = items.Select(c => new ISDN.Models.OrderItem { ProductId = c.ProductId, Quantity = c.Quantity, Subtotal = c.Total }).ToList()
            };

            try
            {
                await _context.Orders.AddAsync(newOrder);
                await _context.SaveChangesAsync();

                // Generate a friendly order number in format ORD-YYYY-XXX where XXX = 100 + OrderId
                try
                {
                    newOrder.OrderNumber = $"ORD-{DateTime.Now:yyyy}-{100 + newOrder.OrderId}";
                    _context.Orders.Update(newOrder);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update OrderNumber for OrderId={OrderId}", newOrder.OrderId);
                }

                // Create Payment
                var payment = new ISDN.Models.Payment
                {
                    OrderId = newOrder.OrderId,
                    RdcId = customer.RdcId,
                    Amount = newOrder.TotalAmount,
                    PaymentMethod = string.IsNullOrEmpty(payment_method) ? "Card" : payment_method,
                    PaymentStatus = "Completed",
                    TransactionId = !string.IsNullOrEmpty(bank_ref) ? bank_ref : Guid.NewGuid().ToString(),
                    PaymentDate = DateTime.Now,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                // ViewBag for Success Page
                ViewBag.OrderNumber = newOrder.OrderNumber;
                ViewBag.TransactionId = payment.TransactionId;
                ViewBag.CustomerName = (customer.first_name + " " + customer.last_name).Trim();

                // Clear Cart
                HttpContext.Session.Remove(CartSessionKey);

                return View("PaymentSuccess", items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save order/payment for user {UserId}", userId);
                TempData["ErrorMessage"] = "There was a problem saving your order/payment. Please contact support.";
                TempData["DbError"] = ex.ToString();
                return View("~/Views/Payment/Index.cshtml", items);
            }
        }
    }
}