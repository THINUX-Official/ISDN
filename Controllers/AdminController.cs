using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Models;

namespace ISDN.Controllers
{
    // [Authorize(Roles = "Admin")] // Uncomment when roles are fully implemented
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // Mock Data for Dashboard
            ViewBag.TotalUsers = 8;
            ViewBag.SystemStatus = "Active";
            ViewBag.RecentLogsCount = 10;
            ViewBag.UserRolesCount = 8;

            ViewBag.RecentActivity = new List<dynamic>
            {
                new { Action = "LOGIN_SUCCESS", Description = "User logged in successfully", Timestamp = DateTime.Now.AddMinutes(-5) },
                new { Action = "LOGIN_SUCCESS", Description = "User logged in successfully", Timestamp = DateTime.Now.AddMinutes(-120) },
                new { Action = "LOGOUT", Description = "User logged out", Timestamp = DateTime.Now.AddMinutes(-123) },
                new { Action = "LOGIN_SUCCESS", Description = "User logged in successfully", Timestamp = DateTime.Now.AddMinutes(-123) },
                new { Action = "LOGOUT", Description = "User logged out", Timestamp = DateTime.Now.AddMinutes(-123) },
                new { Action = "LOGIN_SUCCESS", Description = "User logged in successfully", Timestamp = DateTime.Now.AddHours(-3) },
                new { Action = "LOGOUT", Description = "User logged out", Timestamp = DateTime.Now.AddHours(-3).AddMinutes(6) },
                new { Action = "LOGIN_SUCCESS", Description = "User logged in successfully", Timestamp = DateTime.Now.AddHours(-3).AddMinutes(12) },
                new { Action = "LOGIN_SUCCESS", Description = "User logged in successfully", Timestamp = DateTime.Now.AddHours(-22) },
                new { Action = "LOGOUT", Description = "User logged out", Timestamp = DateTime.Now.AddHours(-26) }
            };

            return View();
        }
    }
}
