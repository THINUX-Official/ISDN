using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;
using ISDN_Distribution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISDN_Distribution.Repositories
{
    public class RdcOrderRepository : IRdcOrderRepository
    {
        private readonly IsdnDbContext _context;

        public RdcOrderRepository(IsdnDbContext context)
        {
            _context = context;
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
                IsPacked = o.Status.Equals("Packed", StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, int adminId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ProcessReturnAsync(int returnId, string status, string adminComment, int adminId)
        {
            var ret = await _context.OrderReturns.FindAsync(returnId);
            if (ret == null) return false;

            ret.AdminStatus = status;
            ret.AdminComment = adminComment;
            ret.ProcessedById = adminId; // කවුද මේක කලේ කියලා Record වෙනවා
            ret.RefundStatus = (status == "APPROVED") ? "PROCESSED" : "REJECTED";

            _context.OrderReturns.Update(ret);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}


