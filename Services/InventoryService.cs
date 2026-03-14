using ISDN.Data;
using ISDN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ISDN.Services
{
    /// <summary>
    /// Service implementation for inventory management operations
    /// Handles stock reservations, transfers, and returns with transactional safety
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IsdnDbContext _context;

        public InventoryService(IsdnDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ReserveStockAsync(int productId, int rdcId, int quantity)
        {
            try
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.RdcId == rdcId);

                if (inventory == null)
                {
                    return false;
                }

                // Check if enough available stock exists
                if (inventory.QuantityAvailable < quantity)
                {
                    return false;
                }

                // Increase reserved quantity
                inventory.QuantityReserved += quantity;
                inventory.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reserving stock: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeductStockOnPackingAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null || order.RdcId == null)
                {
                    return false;
                }

                // Process each order item
                foreach (var item in order.OrderItems)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.RdcId == order.RdcId);

                    if (inventory == null)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Check if reserved quantity is sufficient
                    if (inventory.QuantityReserved < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    // Deduct from both available and reserved
                    inventory.QuantityAvailable -= item.Quantity;
                    inventory.QuantityReserved -= item.Quantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error deducting stock on packing: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ReturnStockToQuarantineAsync(int orderId, int productId, int quantity, int rdcId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if quarantine inventory record exists
                var quarantineInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productId 
                                            && i.RdcId == rdcId 
                                            && i.Location == "RETURNS-HOLD");

                if (quarantineInventory != null)
                {
                    // Update existing quarantine inventory
                    quarantineInventory.QuantityAvailable += quantity;
                    quarantineInventory.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Create new quarantine inventory record
                    var newQuarantineInventory = new Inventory
                    {
                        ProductId = productId,
                        RdcId = rdcId,
                        Location = "RETURNS-HOLD",
                        QuantityAvailable = quantity,
                        QuantityReserved = 0,
                        ReorderLevel = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.Inventories.Add(newQuarantineInventory);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error returning stock to quarantine: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, int rdcId, int quantity)
        {
            try
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productId 
                                            && i.RdcId == rdcId
                                            && (i.Location == null || i.Location != "RETURNS-HOLD"));

                if (inventory == null)
                {
                    return false;
                }

                return inventory.QuantityAvailable >= quantity;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Inventory?> GetInventoryAsync(int productId, int rdcId)
        {
            try
            {
                return await _context.Inventories
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.ProductId == productId 
                                            && i.RdcId == rdcId
                                            && (i.Location == null || i.Location != "RETURNS-HOLD"));
            }
            catch
            {
                return null;
            }
        }
    }
}
