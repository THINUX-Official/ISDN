using ISDN.Data;
using ISDN.Models;
using ISDN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ISDN.Controllers
{
    [Authorize(Roles = "ADMIN,HEAD_OFFICE,RDC_STAFF")]
    public class ReturnProcessingController : Controller
    {
        private readonly IsdnDbContext _context;
        private readonly IConfiguration _config;

        public ReturnProcessingController(IsdnDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: /ReturnProcessing/ProcessReturns
        public async Task<IActionResult> ProcessReturns(string status = "PENDING")
        {
            var returns = await _context.OrderReturns
                .Where(r => r.RefundStatus == status || r.AdminStatus == status || status == "ALL")
                .ToListAsync();

            var itemIds = returns.Select(r => r.ProductId).Distinct().ToList();
            var orderIds = returns.Select(r => r.OrderId).Distinct().ToList();

            var products = await _context.Products.Where(p => itemIds.Contains(p.ProductId)).ToListAsync();
            var orders = await _context.Orders.Where(o => orderIds.Contains(o.OrderId)).ToListAsync();

            // Join into viewmodel list
            var vm = returns.Select(r => new ReturnProcessingViewModel
            {
                ReturnId = r.ReturnId,
                OrderId = r.OrderId,
                OrderNumber = orders.FirstOrDefault(o => o.OrderId == r.OrderId)?.OrderNumber,
                ProductId = r.ProductId,
                ProductName = products.FirstOrDefault(p => p.ProductId == r.ProductId)?.ProductName,
                Quantity = r.Quantity,
                Subtotal = _context.OrderItems.Where(oi => oi.OrderId == r.OrderId && oi.ProductId == r.ProductId).Select(oi => oi.Subtotal).FirstOrDefault(),
                AdminComment = r.AdminComment,
                AdminStatus = r.AdminStatus,
                RefundStatus = r.RefundStatus ?? r.AdminStatus,
                TotalRefundAmount = _context.OrderItems.Where(oi => oi.OrderId == r.OrderId && oi.ProductId == r.ProductId).Select(oi => oi.Subtotal).FirstOrDefault()
            }).ToList();

            // Provide PayPal client id for sandbox usage; can be configured in appsettings under Paypal:ClientId
            ViewBag.PaypalClientId = _config["Paypal:ClientId"] ?? "sb";
            return View("ProcessReturns", vm);
        }

        [HttpGet]
        public async Task<IActionResult> PendingReturnsValidityCheck()
        {
            var pendingReturns = await _context.OrderReturns
                .Where(r => r.AdminStatus == "PENDING" || string.IsNullOrEmpty(r.AdminStatus))
                .ToListAsync();

            var orderIds = pendingReturns.Select(r => r.OrderId).Distinct().ToList();
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => orderIds.Contains(o.OrderId))
                .ToListAsync();

            var viewModels = new List<PendingReturnViewModel>();
            
            foreach(var r in pendingReturns)
            {
                var order = orders.FirstOrDefault(o => o.OrderId == r.OrderId);
                var product = await _context.Products.FindAsync(r.ProductId);

                viewModels.Add(new PendingReturnViewModel
                {
                    ReturnId = r.ReturnId,
                    OrderId = r.OrderId,
                    OrderNumber = order?.OrderNumber,
                    AdminComment = r.AdminComment,
                    Status = r.AdminStatus ?? "PENDING",
                    CustomerName = order?.Customer?.first_name + " " + order?.Customer?.last_name,
                    CustomerEmail = order?.Customer?.email,
                    Items = new List<PendingReturnItemViewModel>
                    {
                        new PendingReturnItemViewModel
                        {
                            ProductName = product?.ProductName ?? "Unknown",
                            Quantity = r.Quantity,
                            Price = product?.UnitPrice ?? 0m
                        }
                    }
                });
            }

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(int returnId)
        {
            var ret = await _context.OrderReturns.FindAsync(returnId);
            if (ret == null) return NotFound();

            // Update inventory: increment quantity_available for the product at the order's RDC if available
            await UpdateInventoryForReturn(ret);

            // Mark return as processed
            ret.RefundStatus = "COMPLETED";
            ret.AdminStatus = "APPROVED";
            ret.ProcessedById = GetCurrentUserId();
            _context.OrderReturns.Update(ret);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Return processed and inventory updated." });
        }

        private int GetCurrentUserId()
        {
            var claim = User?.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
            if (int.TryParse(claim, out int id)) return id;
            return 0;
        }

        private async Task UpdateInventoryForReturn(OrderReturn ret)
        {
            // Find order to detect rdc
            var order = await _context.Orders.FindAsync(ret.OrderId);
            int? rdcId = order?.RdcId;

            // Find inventory record for this product and RDC
            Inventory? inv = null;
            if (rdcId.HasValue)
            {
                inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == ret.ProductId && i.RdcId == rdcId.Value && i.IsActive);
            }

            // Fallback: any active inventory
            if (inv == null)
            {
                inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == ret.ProductId && i.IsActive);
            }

            if (inv != null)
            {
                inv.QuantityAvailable += ret.Quantity;
                inv.LastUpdated = DateTime.UtcNow;
                _context.Inventories.Update(inv);
                await _context.SaveChangesAsync();
            }
            else
            {
                // No inventory row, create one under RDC (if known) or default
                var newInv = new Inventory
                {
                    ProductId = ret.ProductId,
                    RdcId = rdcId,
                    Location = "Returns",
                    QuantityAvailable = ret.Quantity,
                    QuantityReserved = 0,
                    ReorderLevel = 0,
                    LastUpdated = DateTime.UtcNow,
                    IsActive = true
                };
                await _context.Inventories.AddAsync(newInv);
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReturnDetails(int returnId)
        {
            var ret = await _context.OrderReturns.FindAsync(returnId);
            if (ret == null) return NotFound();

            var order = await _context.Orders.FindAsync(ret.OrderId);
            var customer = await _context.Customers.FindAsync(order?.CustomerId ?? 0);
            var product = await _context.Products.FindAsync(ret.ProductId);
            var subtotal = _context.OrderItems.Where(oi => oi.OrderId == ret.OrderId && oi.ProductId == ret.ProductId).Select(oi => oi.Subtotal).FirstOrDefault();

            return Json(new
            {
                returnId = ret.ReturnId,
                orderId = ret.OrderId,
                productId = ret.ProductId,
                productName = product?.ProductName,
                quantity = ret.Quantity,
                subtotal = subtotal,
                totalRefundAmount = subtotal,
                adminComment = ret.AdminComment,
                refundStatus = ret.RefundStatus,
                customer = new { email = customer?.email }
            });
        }
    }
}
