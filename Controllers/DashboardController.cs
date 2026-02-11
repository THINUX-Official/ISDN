using Microsoft.AspNetCore.Mvc;

namespace ISDN.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
