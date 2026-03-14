using ISDN.Data;
using ISDN.Models;
using ISDN_Distribution.Models;
using ISDN_Distribution.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public async Task<IActionResult> MyOrders(int? branchId, string? businessType)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                // Determine customer and userType
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == currentUserId);
                var viewModel = new ISDN_Distribution.Models.CustomerOrdersViewModel();

                if (customer != null)
                {
                    var userType = ISDN.Helpers.AuthHelper.GetValue(customer.business_name, 2);
                    if (string.Equals(userType, "PBOS", StringComparison.OrdinalIgnoreCase) || string.Equals(userType, "PBOM", StringComparison.OrdinalIgnoreCase))
                    {
                        // Provide cluster branches and business types for dropdowns
                        var allCustomers = await _context.Customers.ToListAsync();
                        var uniqueCode = customer.GetRegistrationCode() ?? "SBO_" + customer.CustomerId;
                        var cluster = allCustomers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode).ToList();
                        ViewBag.ClusterBranches = cluster.Select(c => new { c.CustomerId, Name = ISDN.Helpers.AuthHelper.GetValue(c.business_name, 4) ?? c.street_address ?? c.email }).ToList();
                        ViewBag.BusinessTypes = cluster.Select(c => ISDN.Helpers.AuthHelper.GetValue(c.business_name, 1)).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                        ViewBag.SelectedBranchId = branchId;
                        ViewBag.SelectedBusinessType = businessType;
                        viewModel = await _orderRepository.GetClusterOrdersAsync(currentUserId, branchId, businessType);
                    }
                    else
                    {
                        viewModel = await _orderRepository.GetCustomerOrdersAsync(currentUserId);
                    }
                }

                return View(viewModel);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReturn(string orderId, int[] selectedItems, int reasonId, string comments)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            // Search by OrderNumber as passed from the view
            var order = await _context.Orders
                .Include(o => o.Deliveries)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("MyOrders");
            }

            // Calculate return window based on actual delivery date if available
            var deliveryDate = order.Deliveries?
                                .Where(d => d.Status.ToUpper() == "DELIVERED")
                                .OrderByDescending(d => d.DeliveryDate)
                                .Select(d => d.DeliveryDate)
                                .FirstOrDefault() ?? order.OrderDate;

            if ((DateTime.Now - deliveryDate).TotalHours > 72)
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
                            AdminStatus = "PENDING",
                            CreatedAt = DateTime.Now,
                            ReturnType = "REFUND"
                        };
                        _context.OrderReturns.Add(newReturn);
                    }
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your return request has been submitted successfully!";
            }
            else
            {
                TempData["Error"] = "Please select at least one item to return.";
            }

            return RedirectToAction("MyOrders");
        }
    }
}