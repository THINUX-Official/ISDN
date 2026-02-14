using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;
using ISDN_Distribution.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISDN_Distribution.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IsdnDbContext _context;

        public OrderRepository(IsdnDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerOrdersViewModel> GetCustomerOrdersAsync(int userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return new CustomerOrdersViewModel();

            var ordersFromDb = await _context.Orders
                .Where(o => o.CustomerId == customer.CustomerId && o.RdcId == customer.RdcId)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // මේ කොටස අලුතින් එකතු කරන්න: Customer ට අදාළ Returns ටික ගන්නවා
            var orderIds = ordersFromDb.Select(o => o.OrderId).ToList();
            var returns = await _context.OrderReturns
                .Where(r => orderIds.Contains(r.OrderId))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reasons = await _context.ReturnReasons.Select(r => r.ReasonText).ToListAsync();

            return new CustomerOrdersViewModel
            {
                Orders = ordersFromDb,
                ReturnReasons = reasons,
                MyReturns = returns // ViewModel එකට returns ටික දැම්මා
            };
        }

        public async Task<CustomerOrdersViewModel> GetByUserIdAsync(int userId)
        {
            return await GetCustomerOrdersAsync(userId);
        }
    }
} // <--- මෙතන වරහන් දෙකක් අනිවාර්යයි
