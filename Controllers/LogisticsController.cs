using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Controllers
{
    /// <summary>
    /// Logistics Dashboard Controller
    /// Schedules deliveries, tracks shipments, and manages logistics operations with RDC-based filtering
    /// </summary>
    [Authorize(Roles = UserRoles.Logistics)]
    public class LogisticsController : BaseRdcController
    {
        private readonly IsdnDbContext _context;

        public LogisticsController(IsdnDbContext context)
        {
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
        public async Task<IActionResult> Deliveries()
        {
            // Get deliveries with RDC filtering
            var deliveriesQuery = _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.Driver)
                .AsQueryable();

            // Apply RDC filter
            deliveriesQuery = ApplyRdcFilter(deliveriesQuery);

            var deliveries = await deliveriesQuery
                .OrderByDescending(d => d.ScheduledDate)
                .ToListAsync();
            
            ViewBag.RdcId = GetUserRdcId();
            return View(deliveries);
        }

        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            // Get pending orders with RDC filtering
            var ordersQuery = _context.Orders
                .Where(o => o.Status == "Confirmed")
                .Include(o => o.User)
                .AsQueryable();

            // Apply RDC filter
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var pendingOrders = await ordersQuery.ToListAsync();
            
            ViewBag.RdcId = GetUserRdcId();
            return View(pendingOrders);
        }
    }
}
