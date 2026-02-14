using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;

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
            // Get payments with RDC filtering
            var paymentsQuery = _context.Payments
                .Where(p => p.PaymentStatus == "Completed")
                .AsQueryable();

            paymentsQuery = ApplyRdcFilter(paymentsQuery);

            var totalRevenue = await paymentsQuery.SumAsync(p => p.Amount);

            var pendingPaymentsQuery = _context.Payments
                .Where(p => p.PaymentStatus == "Pending")
                .AsQueryable();

            pendingPaymentsQuery = ApplyRdcFilter(pendingPaymentsQuery);

            var pendingPayments = await pendingPaymentsQuery.CountAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.PendingPayments = pendingPayments;
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
    }
}
