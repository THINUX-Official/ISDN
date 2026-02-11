using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Controllers
{
    /// <summary>
    /// Head Office Dashboard Controller
    /// Views reports, KPIs, and manages high-level operations
    /// Head Office users have access to ALL RDC data (rdc_id = NULL)
    /// </summary>
    [Authorize(Roles = UserRoles.HeadOffice)]
    public class HeadOfficeController : BaseRdcController
    {
        private readonly IsdnDbContext _context;

        public HeadOfficeController(IsdnDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.IsHeadOffice = IsHeadOfficeUser();
            ViewBag.RdcId = GetUserRdcId();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            // Head Office sees all orders across all RDCs
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Apply RDC filter (will return all for Head Office)
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var totalOrders = await ordersQuery.CountAsync();
            var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.IsHeadOffice = IsHeadOfficeUser();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> KPIs()
        {
            // Head Office can view KPIs across all RDCs
            var ordersQuery = _context.Orders.AsQueryable();
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var deliveriesQuery = _context.Deliveries.AsQueryable();
            deliveriesQuery = ApplyRdcFilter(deliveriesQuery);

            var paymentsQuery = _context.Payments.AsQueryable();
            paymentsQuery = ApplyRdcFilter(paymentsQuery);

            ViewBag.TotalOrders = await ordersQuery.CountAsync();
            ViewBag.PendingDeliveries = await deliveriesQuery.CountAsync(d => d.Status == "Pending");
            ViewBag.CompletedPayments = await paymentsQuery.CountAsync(p => p.PaymentStatus == "Completed");
            ViewBag.IsHeadOffice = IsHeadOfficeUser();

            return View();
        }
    }
}
