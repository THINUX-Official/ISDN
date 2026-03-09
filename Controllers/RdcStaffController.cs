using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Data;
using ISDN.Models;
using ISDN.Services;
using Microsoft.EntityFrameworkCore;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.RdcStaff)]
    public class RdcStaffController : BaseRdcController
    {
        private readonly IProductRepository _productRepository;
        private readonly IRdcOrderRepository _rdcOrderRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IsdnDbContext _context;

        public RdcStaffController(
            IProductRepository productRepository, 
            IRdcOrderRepository rdcOrderRepository, 
            IInventoryService inventoryService,
            IsdnDbContext context)
        {
            _productRepository = productRepository;
            _rdcOrderRepository = rdcOrderRepository;
            _inventoryService = inventoryService;
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
            var rdcId = GetUserRdcId();
            if (!rdcId.HasValue)
            {
                TempData["Error"] = "RDC information not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Get RDC information
            var rdc = await _context.Rdcs.FindAsync(rdcId.Value);

            // Get all inventory for this RDC
            var inventoryItems = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.RdcId == rdcId.Value)
                .OrderBy(i => i.Product.ProductName)
                .ToListAsync();

            // Separate normal stock from quarantine
            var viewModel = new ISDN.ViewModels.InventoryViewModel
            {
                RdcId = rdcId.Value,
                RdcName = rdc?.RdcName ?? "Unknown RDC",
                NormalStock = inventoryItems
                    .Where(i => string.IsNullOrEmpty(i.Location) || i.Location != "RETURNS-HOLD")
                    .Select(i => new ISDN.ViewModels.InventoryItemViewModel
                    {
                        InventoryId = i.InventoryId,
                        ProductId = i.ProductId,
                        ProductName = i.Product?.ProductName ?? "Unknown",
                        ProductCode = i.Product?.Sku ?? "N/A",
                        Category = i.Product?.Category ?? "N/A",
                        QuantityAvailable = i.QuantityAvailable,
                        QuantityReserved = i.QuantityReserved,
                        ReorderLevel = i.ReorderLevel,
                        Location = i.Location ?? "Main Warehouse",
                        LastUpdated = i.LastUpdated
                    }).ToList(),
                QuarantineStock = inventoryItems
                    .Where(i => i.Location == "RETURNS-HOLD")
                    .Select(i => new ISDN.ViewModels.InventoryItemViewModel
                    {
                        InventoryId = i.InventoryId,
                        ProductId = i.ProductId,
                        ProductName = i.Product?.ProductName ?? "Unknown",
                        ProductCode = i.Product?.Sku ?? "N/A",
                        Category = i.Product?.Category ?? "N/A",
                        QuantityAvailable = i.QuantityAvailable,
                        QuantityReserved = i.QuantityReserved,
                        ReorderLevel = i.ReorderLevel,
                        Location = i.Location ?? "Unknown",
                        LastUpdated = i.LastUpdated
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            int rdcId = GetUserRdcId() ?? 0;

            // ViewModel එක පාවිච්චි කරලා filtered orders ටික ගන්නවා
            var ordersWithReturns = await _rdcOrderRepository.GetOrdersByRdcAsync(rdcId);

            ViewBag.RdcId = rdcId;
            return View(ordersWithReturns);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPacked(int orderId)
        {
            int adminId = GetUserId();
            bool success = await _rdcOrderRepository.UpdateOrderStatusAsync(orderId, "PACKED", adminId);

            if (success)
            {
                TempData["SuccessMessage"] = "Order marked as Packed successfully! Stock has been deducted.";
            }
            else
            {
                TempData["Error"] = "Failed to mark order as packed. Insufficient stock or error occurred.";
            }
            return RedirectToAction(nameof(Orders));
        }

        [HttpGet]
        public async Task<IActionResult> CheckStock(int orderId)
        {
            var rdcId = GetUserRdcId();
            if (!rdcId.HasValue)
            {
                return Json(new { success = false, message = "RDC not found" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.RdcId == rdcId.Value);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            var stockStatus = new List<object>();
            bool allAvailable = true;

            foreach (var item in order.OrderItems)
            {
                var isAvailable = await _inventoryService.CheckStockAvailabilityAsync(
                    item.ProductId, 
                    rdcId.Value, 
                    item.Quantity);

                var inventory = await _inventoryService.GetInventoryAsync(item.ProductId, rdcId.Value);

                stockStatus.Add(new
                {
                    productName = item.Product?.ProductName ?? "Unknown",
                    requiredQuantity = item.Quantity,
                    availableQuantity = inventory?.QuantityAvailable ?? 0,
                    reservedQuantity = inventory?.QuantityReserved ?? 0,
                    isAvailable = isAvailable
                });

                if (!isAvailable)
                {
                    allAvailable = false;
                }
            }

            return Json(new
            {
                success = true,
                allAvailable = allAvailable,
                stockStatus = stockStatus
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(int returnId, string status, string adminComment)
        {
            // ලොග් වී සිටින RDC Staff member ගේ User ID එක මෙතනින් ගන්නවා
            int adminId = GetUserId();

            if (string.IsNullOrEmpty(adminComment))
            {
                TempData["Error"] = "Please provide a comment for your decision.";
                return RedirectToAction(nameof(Orders));
            }

            bool success = await _rdcOrderRepository.ProcessReturnAsync(returnId, status, adminComment, adminId);

            if (success)
            {
                TempData["SuccessMessage"] = $"Return request has been {status.ToLower()} successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to process the return request.";
            }

            return RedirectToAction(nameof(Orders));
        }

        /// <summary>
        /// Move stock from quarantine back to normal inventory
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveQuarantineStock(int inventoryId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var quarantineInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.Location == "RETURNS-HOLD");

                if (quarantineInventory == null)
                {
                    TempData["Error"] = "Quarantine stock not found.";
                    return RedirectToAction(nameof(Inventory));
                }

                // Find or create normal inventory for the same product and RDC
                var normalInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == quarantineInventory.ProductId 
                                            && i.RdcId == quarantineInventory.RdcId 
                                            && (i.Location == null || i.Location != "RETURNS-HOLD"));

                if (normalInventory != null)
                {
                    // Add to existing normal inventory
                    normalInventory.QuantityAvailable += quarantineInventory.QuantityAvailable;
                    normalInventory.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Create new normal inventory record
                    normalInventory = new Inventory
                    {
                        ProductId = quarantineInventory.ProductId,
                        RdcId = quarantineInventory.RdcId,
                        Location = null,
                        QuantityAvailable = quarantineInventory.QuantityAvailable,
                        QuantityReserved = 0,
                        ReorderLevel = quarantineInventory.ReorderLevel,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.Inventories.Add(normalInventory);
                }

                // Remove quarantine record
                _context.Inventories.Remove(quarantineInventory);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Stock moved back to normal inventory successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error approving quarantine stock: {ex.Message}");
                TempData["Error"] = "Failed to move stock back to inventory.";
            }

            return RedirectToAction(nameof(Inventory));
        }

        /// <summary>
        /// Dispose/Remove quarantine stock (for damaged items)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisposeQuarantineStock(int inventoryId)
        {
            try
            {
                var quarantineInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.Location == "RETURNS-HOLD");

                if (quarantineInventory == null)
                {
                    TempData["Error"] = "Quarantine stock not found.";
                    return RedirectToAction(nameof(Inventory));
                }

                _context.Inventories.Remove(quarantineInventory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Quarantine stock disposed successfully!";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing quarantine stock: {ex.Message}");
                TempData["Error"] = "Failed to dispose stock.";
            }

            return RedirectToAction(nameof(Inventory));
        }

        [HttpGet]
        public async Task<IActionResult> GetStockForEdit(int inventoryId)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

                if (inventory == null)
                {
                    return Json(new { success = false, message = "Inventory not found" });
                }

                return Json(new
                {
                    success = true,
                    inventoryId = inventory.InventoryId,
                    productName = inventory.Product?.ProductName ?? "Unknown Product",
                    available = inventory.QuantityAvailable,
                    reserved = inventory.QuantityReserved
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting stock: {ex.Message}");
                return Json(new { success = false, message = "Error loading stock data" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int inventoryId, int addQuantity, int newReserved)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

                if (inventory == null)
                {
                    return Json(new { success = false, message = "Inventory not found" });
                }

                inventory.QuantityAvailable += addQuantity;
                inventory.QuantityReserved = newReserved;
                inventory.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Stock updated successfully",
                    newAvailable = inventory.QuantityAvailable,
                    newReserved = inventory.QuantityReserved
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stock: {ex.Message}");
                return Json(new { success = false, message = "Error updating stock" });
            }
        }
    }
}
