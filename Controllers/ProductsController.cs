using Microsoft.AspNetCore.Mvc;

namespace ISDN.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
