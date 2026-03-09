using ISDN.Data;
using ISDN.Models;
using ISDN_Distribution.Repositories; // Ensure this matches your project
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace ISDN.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")] 
    public class DriverController : Controller
    {
        private readonly IsdnDbContext _context;
        private readonly IOrderRepository _orderRepository;

        public DriverController(IsdnDbContext context, IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Fetches the ID from the claim we just created in AccountController
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            int currentDriverId = int.Parse(userIdClaim.Value);

            // Get the driver's specific RDC from the database
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentDriverId);
            if (currentUser == null) return NotFound();

            int currentRdcId = currentUser.RdcId ?? 0;

            // Fetch ONLY the tasks assigned to THIS driver at THIS RDC
            var allDeliveries = await _context.Deliveries
                .Include(d => d.Order).ThenInclude(o => o.Customer)
                .Where(d => d.DriverId == currentDriverId && d.RdcId == currentRdcId)
                .ToListAsync();

            var viewModel = new DriverDashboardViewModel
            {
                ActiveDeliveries = allDeliveries.Where(d => d.Status.ToUpper() != "DELIVERED"),
                CompletedDeliveries = allDeliveries.Where(d => d.Status.ToUpper() == "DELIVERED")
                                        .OrderByDescending(d => d.DeliveryDate),

                TodayCount = allDeliveries.Count(d => d.CreatedAt.Date == DateTime.Today),

                // Fix: Added "IN TRANSIT" and "IN_TRANSIT" to match your database data
                PendingCount = allDeliveries.Count(d =>
                    d.Status.ToUpper() == "PENDING" ||
                    d.Status.ToUpper() == "ON_THE_WAY" ||
                    d.Status.ToUpper() == "IN TRANSIT" ||
                    d.Status.ToUpper() == "IN_TRANSIT"),

                CompletedTodayCount = allDeliveries.Count(d => d.Status.ToUpper() == "DELIVERED" && d.DeliveryDate?.Date == DateTime.Today)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int deliveryId)
        {
            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.DeliveryId == deliveryId);

            if (delivery != null && delivery.DriverId.HasValue)
            {
                delivery.Status = "DELIVERED";
                delivery.DeliveryDate = DateTime.Now;

                if (delivery.Order != null)
                {
                    delivery.Order.Status = "DELIVERED";
                }

                // Fix: Use the actual driver ID from the delivery record to satisfy FK constraint
                _context.OrderStatusLogs.Add(new OrderStatusLog
                {
                    OrderId = delivery.OrderId,
                    Status = "DELIVERED",
                    UpdatedById = delivery.DriverId.Value, // Use .Value instead of ?? 0
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }
    }
}