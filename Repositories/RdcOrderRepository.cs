using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;
using ISDN_Distribution.Models;
using ISDN.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISDN_Distribution.Repositories
{
    public class RdcOrderRepository : IRdcOrderRepository
    {
        private readonly IsdnDbContext _context;
        private readonly IInventoryService _inventoryService;

        public RdcOrderRepository(IsdnDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<List<AdminOrderViewModel>> GetOrdersByRdcAsync(int rdcId)
        {
            var orders = await _context.Orders
                .Where(o => o.RdcId == rdcId)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderIds = orders.Select(o => o.OrderId).ToList();

            var allReturns = await _context.OrderReturns
                .Where(r => orderIds.Contains(r.OrderId))
                .ToListAsync();

            // RdcOrderRepository.cs ඇතුළේ mapping එක මෙහෙම වෙනස් කරන්න
            return orders.Select(o => new AdminOrderViewModel
            {
                Order = o,
                // AdminStatus එක 'PENDING' තියෙන රිටර්න්ස් විතරක් ගමු. 
                // Approve හෝ Reject කළාම මේ status එක වෙනස් වෙන නිසා ඉබේම tab එකෙන් අයින් වෙනවා.
                ActiveReturns = allReturns
                    .Where(r => r.OrderId == o.OrderId && r.AdminStatus == "PENDING")
                    .ToList(),

                IsStockAvailable = true,
                IsPacked = o.Status.Equals("PACKED", StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, int adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                order.Status = status;
                _context.Orders.Update(order);

                // If order is marked as PACKED, deduct inventory
                if (status.Equals("PACKED", StringComparison.OrdinalIgnoreCase))
                {
                    var stockDeducted = await _inventoryService.DeductStockOnPackingAsync(orderId);
                    if (!stockDeducted)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error updating order status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessReturnAsync(int returnId, string status, string adminComment, int adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ret = await _context.OrderReturns
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.ReturnId == returnId);

                if (ret == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                ret.AdminStatus = status;
                ret.AdminComment = adminComment;
                ret.ProcessedById = adminId;
                ret.RefundStatus = (status == "APPROVED") ? "PROCESSED" : "REJECTED";

                _context.OrderReturns.Update(ret);

                // If return is approved, add stock back to quarantine
                if (status == "APPROVED")
                {
                    var order = await _context.Orders.FindAsync(ret.OrderId);
                    if (order != null && order.RdcId.HasValue)
                    {
                        var stockReturned = await _inventoryService.ReturnStockToQuarantineAsync(
                            ret.OrderId, 
                            ret.ProductId, 
                            ret.Quantity, 
                            order.RdcId.Value);

                        if (!stockReturned)
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"Error processing return: {ex.Message}");
                return false;
            }
        }
    }
}


