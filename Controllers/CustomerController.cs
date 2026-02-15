using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISDN.Controllers
{
    // [Authorize(Roles = "Customer")] // Uncomment when roles are implemented
    public class CustomerController : Controller
    {
        public IActionResult Dashboard()
        {
            // Placeholder: functionality to get current user details
            ViewBag.UserName = User.Identity?.Name ?? "Customer";
            return View();
        }

        public IActionResult Orders()
        {
            ViewBag.Message = "Order History functionality coming soon.";
            return View("Placeholder");
        }

        public IActionResult Payments()
        {
            ViewBag.Message = "Payment History functionality coming soon.";
            return View("Placeholder");
        }

        public IActionResult Profile()
        {
            ViewBag.Message = "Profile Settings functionality coming soon.";
            return View("Placeholder");
        }
    }
}
