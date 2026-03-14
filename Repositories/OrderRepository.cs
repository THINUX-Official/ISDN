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
                .Include(o => o.Deliveries) // <--- CRITICAL FIX: Add this line
                .Include(o => o.OrderStatusLogs)
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

        // New: Get orders across a customer's cluster (PBOS/PBOM) with optional filters
        public async Task<CustomerOrdersViewModel> GetClusterOrdersAsync(int userId, int? branchId = null, string? businessType = null)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return new CustomerOrdersViewModel();

            // Materialize customers to use GetRegistrationCode()
            var allCustomers = await _context.Customers.ToListAsync();
            var uniqueCode = customer.GetRegistrationCode() ?? "SBO_" + customer.CustomerId;
            var clusterMembers = allCustomers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode).ToList();

            if (branchId.HasValue)
            {
                clusterMembers = clusterMembers.Where(c => c.CustomerId == branchId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(businessType))
            {
                clusterMembers = clusterMembers.Where(c => ISDN.Helpers.AuthHelper.GetValue(c.business_name, 1).Equals(businessType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var branchIds = clusterMembers.Select(c => c.CustomerId).ToList();

            var ordersFromDb = await _context.Orders
                .Where(o => o.CustomerId.HasValue && branchIds.Contains(o.CustomerId.Value))
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Deliveries)
                .Include(o => o.OrderStatusLogs)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

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
                MyReturns = returns
            };
        }

        public async Task<List<Order>> GetOrdersByStatusAndRdcAsync(string status, int rdcId)
        {
            return await _context.Orders
                .Where(o => o.Status == status && o.RdcId == rdcId)
                .Include(o => o.Customer)
                .Include(o => o.OrderStatusLogs) // Include logs to get the real update time
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, int updatedById)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                // DEBUG: Print RDC info to Output Window
                System.Diagnostics.Debug.WriteLine($"DEBUG: Order {orderId} belongs to RDC: {order.RdcId}");

                order.Status = newStatus;

                var log = new OrderStatusLog
                {
                    OrderId = orderId,
                    Status = newStatus,
                    UpdatedById = updatedById,
                    CreatedAt = DateTime.Now
                };
                _context.OrderStatusLogs.Add(log);

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                // THIS IS THE MISSING PIECE. Check your Visual Studio Output Window.
                System.Diagnostics.Debug.WriteLine($"DB ERROR: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetActiveDriversByRdcAsync(int rdcId)
        {
            // RoleId 5 Drivers
            return await _context.Users
                .Where(u => u.RoleId == 5 && u.RdcId == rdcId && u.IsActive == true)
                .ToListAsync();
        }


        public async Task<List<Order>> GetOnTheWayOrdersByRdcAsync(int rdcId)
        {
            return await _context.Orders
                .Where(o => o.Status == "ON_THE_WAY" && o.RdcId == rdcId)
                .Include(o => o.Customer) 
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }


        public async Task<List<Delivery>> GetDriverTasksAsync(int driverId, int rdcId)
        {
            // Use ToListAsync to execute the query immediately
            return await _context.Deliveries
                .Include(d => d.Order)
                    .ThenInclude(o => o.Customer)
                .Where(d => d.DriverId == driverId && d.RdcId == rdcId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AssignDriverAndDispatchAsync(int orderId, int driverId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                order.Status = "ON_THE_WAY";

                var delivery = new Delivery
                {
                    OrderId = orderId,
                    RdcId = order.RdcId,
                    DriverId = driverId,
                    Status = "In Transit",
                    ScheduledDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                var log = new OrderStatusLog
                {
                    OrderId = orderId,
                    Status = "ON_THE_WAY",
                    UpdatedById = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Deliveries.Add(delivery);
                _context.OrderStatusLogs.Add(log);
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Print the specific database error
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                System.Diagnostics.Debug.WriteLine($"DB CONSTRAINT ERROR: {message}");

                return false;
            }
        }

    }
} 