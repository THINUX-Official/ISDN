using Microsoft.AspNetCore.Mvc;
using ISDN.Models;
using ISDN.Repositories;
using ISDN.Data;
using ISDN.Extensions;

namespace ISDN.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IsdnDbContext _context;

        public CartController(IProductRepository productRepository, IsdnDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new List<OrderItem>();

            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
                existingItem.Subtotal = existingItem.Quantity * product.UnitPrice;
            }
            else
            {
                cart.Add(new OrderItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Subtotal = product.UnitPrice,
                    Product = product
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["SuccessMessage"] = "Item added to cart!";
            return RedirectToAction("Index"); // Or redirect to referring page
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    OrderNumber = "ORD-" + DateTime.Now.Ticks, // Simple order number generation
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Sum(i => i.Subtotal),
                    UserId = 1, // Placeholder: Replace with User.Identity.GetUserId() when Auth is ready
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Subtotal = item.Subtotal
                    };
                    _context.OrderItems.Add(orderItem);

                    // Optional: Update stock quantity
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove("Cart");
                return View("OrderSuccess", order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "An error occurred while processing your order: " + ex.Message);
                return View("Index", cart);
            }
        }
    }
}