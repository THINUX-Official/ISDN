using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

namespace ISDN.Controllers
{
    /// <summary>
    /// Finance Dashboard Controller
    /// Manages payments, invoices, and financial reports with RDC-based filtering
    /// </summary>
    [Authorize(Roles = UserRoles.Finance)]
    public class FinanceController : BaseRdcController
    {
        private readonly IsdnDbContext _context;

        public FinanceController(IsdnDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Total Revenue
            var paymentsQuery = _context.Payments
                .Where(p => p.PaymentStatus == "Completed")
                .AsQueryable();

            paymentsQuery = ApplyRdcFilter(paymentsQuery);

            var totalRevenue = await paymentsQuery.SumAsync(p => p.Amount);

            // Pending Payments
            var pendingPaymentsQuery = _context.Payments
                .Where(p => p.PaymentStatus == "Pending")
                .AsQueryable();

            pendingPaymentsQuery = ApplyRdcFilter(pendingPaymentsQuery);

            var pendingPayments = await pendingPaymentsQuery.CountAsync();

            // Invoices Today (Orders created today)
            var invoicesTodayQuery = _context.Orders.AsQueryable();

            invoicesTodayQuery = ApplyRdcFilter(invoicesTodayQuery);

            var invoicesToday = await invoicesTodayQuery
                .Where(o => o.OrderDate.Date == DateTime.Today)
                .CountAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.InvoicesToday = invoicesToday;

            ViewBag.RdcId = GetUserRdcId();
            ViewBag.IsHeadOffice = IsHeadOfficeUser();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Payments()
        {
            // Get payments with RDC filtering
            var paymentsQuery = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.User)
                .AsQueryable();

            // Apply RDC filter
            paymentsQuery = ApplyRdcFilter(paymentsQuery);

            var payments = await paymentsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.RdcId = GetUserRdcId();
            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            // Get orders with RDC filtering
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payments)
                .AsQueryable();

            // Apply RDC filter
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.RdcId = GetUserRdcId();
            return View(orders);
        }

        [HttpGet]
        [Authorize(Roles = "FINANCE,ADMIN")]
        public async Task<IActionResult> AdminRevenue()
        {
            // Determine RDC context for the logged-in user
            var userRdc = GetUserRdcId();
            var isAdminOrHead = User.IsInRole(UserRoles.Admin) || IsHeadOfficeUser();

            // Payments - include Order and filter by RDC
            var paymentsQuery = _context.Payments.Include(p => p.Order).AsQueryable();
            if (!isAdminOrHead && userRdc.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => (p.RdcId.HasValue && p.RdcId == userRdc) || (p.Order != null && p.Order.RdcId == userRdc));
            }

            // Total Sales: only payments marked as Paid/Successful
            var totalSales = await paymentsQuery
                .Where(p => p.PaymentStatus != null && (
                    p.PaymentStatus.ToLower() == "paid" || p.PaymentStatus.ToLower() == "completed" || p.PaymentStatus.ToLower() == "successful"))
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            // Returns: calculate returned item value by joining order_returns -> order_items -> orders
            var returnsQuery = from r in _context.OrderReturns
                               join oi in _context.OrderItems on new { r.OrderId, r.ProductId } equals new { oi.OrderId, oi.ProductId }
                               join o in _context.Orders on r.OrderId equals o.OrderId
                               select new { Return = r, OrderItem = oi, Order = o };

            if (!isAdminOrHead && userRdc.HasValue)
            {
                returnsQuery = returnsQuery.Where(x => x.Order.RdcId == userRdc);
            }

            // compute returned value: unit price = subtotal / quantity, times returned quantity
            var totalReturns = await returnsQuery
                .Where(x => x.OrderItem.Quantity > 0)
                .Select(x => (decimal?)( (x.OrderItem.Subtotal / (decimal)x.OrderItem.Quantity) * x.Return.Quantity ))
                .SumAsync() ?? 0m;

            // Customers count (active) filtered by RDC
            var customersQuery = _context.Customers.AsQueryable();
            if (!isAdminOrHead && userRdc.HasValue)
            {
                customersQuery = customersQuery.Where(c => c.RdcId == userRdc);
            }
            var customerCount = await customersQuery.CountAsync(c => c.IsActive);

            // Completed orders (DELIVERED)
            var ordersQuery = _context.Orders.AsQueryable();
            if (!isAdminOrHead && userRdc.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.RdcId == userRdc);
            }
            var completedOrders = await ordersQuery.CountAsync(o => o.Status != null && o.Status.ToUpper() == "DELIVERED");

            var viewModel = new ISDN.ViewModels.AdminRevenueViewModel
            {
                TotalSales = totalSales,
                TotalReturns = totalReturns,
                NetRevenue = totalSales - totalReturns,
                CustomerCount = customerCount,
                CompletedOrders = completedOrders,
                RdcId = userRdc ?? 0
            };

            if (userRdc.HasValue)
            {
                var rdc = await _context.Rdcs.FindAsync(userRdc.Value);
                if (rdc != null) viewModel.RdcName = rdc.RdcName;
            }

            // Return the Finance view placed in Views/Finance/AdminRevenue.cshtml
            return View("AdminRevenue", viewModel);
        }
    }
}
