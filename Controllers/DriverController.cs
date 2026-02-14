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
    /// Driver Dashboard Controller
    /// Views and updates only assigned deliveries with RDC-based filtering
    /// </summary>
    [Authorize(Roles = UserRoles.Driver)]
    public class DriverController : BaseRdcController
    {
        private readonly IsdnDbContext _context;

        public DriverController(IsdnDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Safely retrieve user ID from JWT claims
            var driverId = GetUserId();
            if (driverId == 0)
            {
                // Invalid or missing user_id claim - clear cookie and redirect to login
                Response.Cookies.Delete("AuthToken");
                TempData["ErrorMessage"] = "Your session has expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // Validate that the driver's RDC ID claim exists
            var driverRdcId = GetUserRdcId();
            if (!driverRdcId.HasValue)
            {
                // Driver must have a valid RDC assignment - clear cookie
                Response.Cookies.Delete("AuthToken");
                TempData["ErrorMessage"] = "Your account is not assigned to an RDC. Please contact the administrator.";
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // Get only deliveries assigned to this driver within their RDC
                var myDeliveries = await _context.Deliveries
                    .Include(d => d.Order)
                        .ThenInclude(o => o.User)
                    .Where(d => d.DriverId == driverId && d.RdcId == driverRdcId)
                    .OrderByDescending(d => d.ScheduledDate)
                    .ToListAsync();

                // Calculate dashboard metrics
                ViewBag.TodayDeliveries = myDeliveries.Count(d => d.ScheduledDate?.Date == DateTime.Today);
                ViewBag.PendingDeliveries = myDeliveries.Count(d => d.Status == "Pending" || d.Status == "In Transit");
                ViewBag.RdcId = driverRdcId;

                return View(myDeliveries);
            }
            catch (Exception ex)
            {
                // Log error, clear cookie, and redirect with friendly message
                Response.Cookies.Delete("AuthToken");
                TempData["ErrorMessage"] = "Unable to load deliveries. Please try again or contact support.";
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyDeliveries()
        {
            // Safely retrieve user ID from JWT claims
            var driverId = GetUserId();
            if (driverId == 0)
            {
                Response.Cookies.Delete("AuthToken");
                TempData["ErrorMessage"] = "Your session has expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // Validate that the driver's RDC ID claim exists
            var driverRdcId = GetUserRdcId();
            if (!driverRdcId.HasValue)
            {
                Response.Cookies.Delete("AuthToken");
                TempData["ErrorMessage"] = "Your account is not assigned to an RDC. Please contact the administrator.";
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // Get only deliveries assigned to this driver within their RDC
                var deliveries = await _context.Deliveries
                    .Include(d => d.Order)
                        .ThenInclude(o => o.User)
                    .Where(d => d.DriverId == driverId && d.RdcId == driverRdcId)
                    .ToListAsync();

                ViewBag.RdcId = driverRdcId;
                return View(deliveries);
            }
            catch (Exception ex)
            {
                Response.Cookies.Delete("AuthToken");
                TempData["ErrorMessage"] = "Unable to load deliveries. Please try again or contact support.";
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
