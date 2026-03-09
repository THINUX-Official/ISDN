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
    /// Logistics Dashboard Controller
    /// Schedules deliveries, tracks shipments, and manages logistics operations with RDC-based filtering
    /// </summary>
    [Authorize(Roles = UserRoles.Logistics)]
    public class LogisticsController : BaseRdcController
    {
        private readonly IsdnDbContext _context;
        private readonly IOrderRepository _orderRepository; // Repository එක මෙතනට එකතු කළා

        // Constructor එක හරහා Context සහ Repository යන දෙකම ලබාගන්නවා
        public LogisticsController(IsdnDbContext context, IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
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
            // Ensure the query includes all deliveries for the RDC, not just one driver
            var deliveriesQuery = _context.Deliveries
                .Include(d => d.Order)
                .Include(d => d.Driver)
                .AsQueryable();

            // Ensure ApplyRdcFilter(deliveriesQuery) allows all RDC orders
            deliveriesQuery = ApplyRdcFilter(deliveriesQuery);

            var deliveries = await deliveriesQuery
                .OrderByDescending(d => d.ScheduledDate)
                .ToListAsync();

            return View(deliveries); // Pass the full list to the View
        }

        // --- ලැබුණු ඇණවුම් භාරගැනීමේ කොටස (Acknowledgment) ---
        [HttpGet]
        public async Task<IActionResult> AcknowledgeOrders()
        {
            int rdcId = GetUserRdcId() ?? 0;
            var viewModel = new LogisticsAcknowledgmentViewModel
            {
                PendingPackedOrders = await _orderRepository.GetOrdersByStatusAndRdcAsync("PACKED", rdcId),
                RecentlyAcknowledgedOrders = await _orderRepository.GetOrdersByStatusAndRdcAsync("RECEIVED_FOR_DELIVERY", rdcId)
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAcknowledgment(int orderId)
        {
            // Use the same helper method defined in your BaseRdcController
            int currentUserId = GetUserId();

            if (currentUserId == 0) // Based on your GetUserId implementation
            {
                TempData["Error"] = "Unable to retrieve your user profile (User ID is 0).";
                return RedirectToAction("AcknowledgeOrders");
            }

            var success = await _orderRepository.UpdateOrderStatusAsync(orderId, "RECEIVED_FOR_DELIVERY", currentUserId);

            if (!success)
            {
                TempData["Error"] = "Database rejected the acknowledgment. Check system logs.";
            }
            else
            {
                TempData["Success"] = $"Order {orderId} acknowledged.";
            }

            return RedirectToAction("AcknowledgeOrders");
        }


        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            int rdcId = GetUserRdcId() ?? 0;
            var viewModel = new LogisticsScheduleViewModel
            {
                ReceivedForDeliveryOrders = await _orderRepository.GetOrdersByStatusAndRdcAsync("RECEIVED_FOR_DELIVERY", rdcId),
                OnTheWayOrders = await _orderRepository.GetOrdersByStatusAndRdcAsync("ON_THE_WAY", rdcId),
                ActiveDrivers = await _orderRepository.GetActiveDriversByRdcAsync(rdcId)
            };

            ViewBag.RdcId = rdcId;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Ensure this is present
        public async Task<IActionResult> AssignDriver(int orderId, int driverId)
        {
            // Consistency fix: Use your base controller helper
            int currentDispatcherId = GetUserId();

            if (currentDispatcherId == 0)
            {
                TempData["Error"] = "Unauthorized: Could not identify dispatcher.";
                return RedirectToAction("Schedule");
            }

            var success = await _orderRepository.AssignDriverAndDispatchAsync(orderId, driverId, currentDispatcherId);

            if (success)
            {
                TempData["Success"] = "Order dispatched successfully!";
            }
            else
            {
                TempData["Error"] = "Database rejected the update. Check driver availability and RDC matching.";
            }

            return RedirectToAction("Schedule");
        }
    }
}
