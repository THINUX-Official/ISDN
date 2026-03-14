using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;
using ISDN.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ISDN.Controllers
{
    public class StockTransferController : Controller
    {
        private readonly IsdnDbContext _context;

        public StockTransferController(IsdnDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> StockManagement()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int rdcId = 0;
            // Assuming we determine if they are superadmin or specific rdC. We will just load everything for the dashboard or filter by RDC based on context. 
            // The instructions imply an overview dashboard.

            var pendingTransfers = await _context.StockTransfers
                .Include(st => st.Product)
                .Include(st => st.FromRdc)
                .Include(st => st.ToRdc)
                .Where(st => st.Status == "PENDING")
                .ToListAsync();

            var completedTransfers = await _context.StockTransfers
                .Include(st => st.Product)
                .Include(st => st.FromRdc)
                .Include(st => st.ToRdc)
                .Where(st => st.Status == "COMPLETED")
                .ToListAsync();

            var lowStock = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Rdc)
                .Where(i => i.QuantityAvailable <= i.ReorderLevel && i.Rdc != null && i.IsActive)
                .ToListAsync();

            var deactivatedRdcs = await _context.Rdcs.Where(r => !r.IsActive).ToListAsync();
            var activeRdcs = await _context.Rdcs.Where(r => r.IsActive).ToListAsync();

            var vm = new StockTransferDashboardViewModel
            {
                PendingTransfers = pendingTransfers,
                CompletedTransfers = completedTransfers,
                OutgoingTransfers = pendingTransfers.ToList(), // Simulating all as overview
                IncomingTransfers = pendingTransfers.ToList(),
                LowStockInventory = lowStock,
                TotalInTransitQuantity = pendingTransfers.Sum(p => p.Quantity),
                LowStockAlertsCount = lowStock.Count,
                DeactivatedRdcs = deactivatedRdcs,
                ActiveRdcs = activeRdcs
            };

            ViewBag.ActiveRdcs = activeRdcs;
            ViewBag.Products = await _context.Products.Where(p => p.IsActive).ToListAsync();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransfer(CreateTransferViewModel model)
        {
            if (model.FromRdcId == model.ToRdcId)
            {
                TempData["Error"] = "Source and Destination RDC cannot be the same.";
                return RedirectToAction("StockManagement");
            }

            var sourceInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.RdcId == model.FromRdcId && i.ProductId == model.ProductId);

            if (sourceInventory == null || sourceInventory.QuantityAvailable < model.Quantity)
            {
                TempData["Error"] = "Insufficient stock at the Source RDC.";
                return RedirectToAction("StockManagement");
            }

            // Reserve Logic
            sourceInventory.QuantityAvailable -= model.Quantity;
            sourceInventory.QuantityReserved += model.Quantity;

            var transfer = new StockTransfer
            {
                ProductId = model.ProductId,
                FromRdcId = model.FromRdcId,
                ToRdcId = model.ToRdcId,
                Quantity = model.Quantity,
                Status = "PENDING"
            };

            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Transfer created successfully and stock reserved.";
            return RedirectToAction("StockManagement");
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTransfer(int transferId)
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.FromRdc)
                .Include(t => t.ToRdc)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null || transfer.Status != "PENDING")
            {
                TempData["Error"] = "Invalid transfer request.";
                return RedirectToAction("StockManagement");
            }

            var sourceInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.RdcId == transfer.FromRdcId && i.ProductId == transfer.ProductId);
                
            var destInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.RdcId == transfer.ToRdcId && i.ProductId == transfer.ProductId);

            // Completion Logic: Remove from Reserved on source, Add to Available on dest
            if (sourceInventory != null)
            {
                sourceInventory.QuantityReserved -= transfer.Quantity;
                if (sourceInventory.QuantityReserved < 0) sourceInventory.QuantityReserved = 0;
            }

            if (destInventory != null)
            {
                destInventory.QuantityAvailable += transfer.Quantity;
            }
            else
            {
                _context.Inventories.Add(new Inventory
                {
                    RdcId = transfer.ToRdcId,
                    ProductId = transfer.ProductId,
                    QuantityAvailable = transfer.Quantity,
                    QuantityReserved = 0,
                    IsActive = true,
                    ReorderLevel = 10 
                });
            }

            transfer.Status = "COMPLETED";
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Transfer completed. Stock updated successfully.";
            return RedirectToAction("StockManagement");
        }
        
        [HttpPost]
        public async Task<IActionResult> LiquidateRdc(int rdcId)
        {
            var rdc = await _context.Rdcs.FindAsync(rdcId);
            if (rdc == null || rdc.IsActive)
            {
                TempData["Error"] = "Invalid RDC selected for liquidation.";
                return RedirectToAction("StockManagement");
            }

            var activeRdcs = await _context.Rdcs.Where(r => r.IsActive).ToListAsync();
            if (!activeRdcs.Any())
            {
                TempData["Error"] = "No active RDCs available to receive liquidated stock.";
                return RedirectToAction("StockManagement");
            }

            var inventories = await _context.Inventories
                .Where(i => i.RdcId == rdcId && i.QuantityAvailable > 0)
                .ToListAsync();

            if (!inventories.Any())
            {
                TempData["Error"] = "Deactivated RDC has no stock to liquidate.";
                return RedirectToAction("StockManagement");
            }

            foreach (var inv in inventories)
            {
                int partQty = inv.QuantityAvailable / activeRdcs.Count;
                int remQty = inv.QuantityAvailable % activeRdcs.Count;

                if(partQty == 0 && remQty > 0) 
                {
                   partQty = inv.QuantityAvailable;
                   remQty = 0;
                   var target = activeRdcs.First();
                   
                   inv.QuantityAvailable -= partQty;
                   inv.QuantityReserved += partQty;

                   _context.StockTransfers.Add(new StockTransfer
                   {
                       ProductId = inv.ProductId,
                       FromRdcId = rdcId,
                       ToRdcId = target.RdcId,
                       Quantity = partQty,
                       Status = "PENDING"
                   });
                   continue;
                }

                foreach (var target in activeRdcs)
                {
                    int transferQty = partQty;
                    if (target == activeRdcs.First()) transferQty += remQty;

                    if (transferQty > 0)
                    {
                        inv.QuantityAvailable -= transferQty;
                        inv.QuantityReserved += transferQty;

                        _context.StockTransfers.Add(new StockTransfer
                        {
                            ProductId = inv.ProductId,
                            FromRdcId = rdcId,
                            ToRdcId = target.RdcId,
                            Quantity = transferQty,
                            Status = "PENDING"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Liquidation transfers automatically generated and left as PENDING.";
            return RedirectToAction("StockManagement");
        }
    }
}