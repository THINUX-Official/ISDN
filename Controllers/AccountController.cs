using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISDN.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectBasedOnRole();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // HARDCODED CREDENTIALS FOR DEMO (Replace with DB check later)
            // Admin: admin@isdn.lk / admin123
            // Customer: customer@test.com / cust123

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter email and password.";
                return View();
            }

            ClaimsIdentity? identity = null;

            if (email == "admin@isdn.lk" && password == "admin123")
            {
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "System Administrator"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Admin")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else if (email == "customer@test.com" && password == "cust123")
            {
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "Kamani Perera"), // Matches dashboard screenshot
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Customer")
                }, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                ViewBag.Error = "Invalid login credentials.";
                return View();
            }

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectBasedOnRole();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectBasedOnRole()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (User.IsInRole("Customer"))
            {
                return RedirectToAction("Dashboard", "Customer");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
