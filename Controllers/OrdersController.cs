using ISDN.Models;
using ISDN.Data; // ISDN_Distribution.Data වෙනුවට
using ISDN_Distribution.Models;
using ISDN_Distribution.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // FirstOrDefaultAsync සඳහා අනිවාර්යයි
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq; // Select, FirstOrDefault සඳහා
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ISDN_Distribution.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersController> _logger;
        private readonly IsdnDbContext _context;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger, IsdnDbContext context)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> MyOrders()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                var viewModel = await _orderRepository.GetCustomerOrdersAsync(currentUserId);
                return View(viewModel);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReturn(string orderId, int[] selectedItems, int reasonId, string comments)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            // OrderNumber එකෙන් නෙවෙයි, OrderId (Primary Key) එකෙන් සර්ච් කරලා බලන්න 
            // නැත්නම් OrderNumber එක හරියටම Database එකේ තියෙනවද බලන්න
            var order = await _context.Orders
                .AsNoTracking() // Performance වලට හොඳයි
                .FirstOrDefaultAsync(o => o.OrderNumber == orderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("MyOrders");
            }

            // පැය 72 සීමාව පරීක්ෂා කිරීම
            if ((DateTime.Now - order.OrderDate).TotalHours > 72)
            {
                TempData["Error"] = "Return period (72h) has expired.";
                return RedirectToAction("MyOrders");
            }

            if (selectedItems != null && selectedItems.Length > 0)
            {
                foreach (var itemId in selectedItems)
                {
                    var itemDetail = await _context.OrderItems.FindAsync(itemId);
                    if (itemDetail != null)
                    {
                        var newReturn = new OrderReturn
                        {
                            OrderId = order.OrderId,
                            ProductId = itemDetail.ProductId,
                            Quantity = itemDetail.Quantity,
                            ReasonId = reasonId,
                            OtherReasonDescription = (reasonId == 4) ? comments : null,
                            RefundStatus = "PENDING",
                            AdminStatus = "PENDING", // මේක අනිවාර්යයෙන් දාන්න
                            CreatedAt = DateTime.Now,
                            ReturnType = "REFUND"
                        };
                        _context.OrderReturns.Add(newReturn);
                    }
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your return request has been submitted successfully!";
            }
            return RedirectToAction("MyOrders");
        }
    }
}