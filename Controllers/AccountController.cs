using ISDN.Constants;
using ISDN.Data;
using ISDN.Models;
using ISDN.Services;
using ISDN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;


namespace ISDN.Controllers
{
    /// <summary>
    /// AccountController handles user authentication with JWT tokens.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly IAuditLogService _auditService;
        private readonly ILogger<AccountController> _logger;
        private readonly IsdnDbContext _context; // මේක අලුතෙන් එකතු කළා

        public AccountController(
            IAuthenticationService authService,
            IAuditLogService auditService,
            ILogger<AccountController> logger,
            IsdnDbContext context) // DbContext එක මෙතනටත් එකතු කළා
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
            _context = context; // මෙතනදී assign කරගන්නවා
        }

        #region Register

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// Updated to handle FirstName and LastName instead of FullName
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Password එක Hash කිරීම (RegisterViewModel එකේ තියෙන ප්ලේන් පාස්වර්ඩ් එක)
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    // 2. Customer object එකක් නිර්මාණය කිරීම
                    // සටහන: Properties වල අකුරු (Capital/Simple) ඔයාගේ Customer.cs එකට අනුව බලන්න
                    var newCustomer = new Customer
                    {
                        first_name = model.FirstName,
                        last_name = model.LastName,
                        email = model.Email,
                        phone_number = model.PhoneNumber,
                        business_name = model.BusinessName,
                        street_address = model.StreetAddress,
                        city = model.City,
                        zip_code = model.ZipCode,
                        temp_password_hash = hashedPassword,
                        registration_status = "PENDING",
                        IsActive = false,
                        CreatedAt = DateTime.Now
                    };

                    // 3. Customers table එකට දත්ත එකතු කර සේව් කිරීම
                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"New pending registration: {model.Email}");

                    TempData["SuccessMessage"] = "Registration request sent! You will be notified once approved.";
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during customer registration.");
                    ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                }
            }

            return View(model);
        }

        #endregion

        #region Login

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _authService.LoginAsync(model.Email, model.Password, ipAddress);

                if (result.Success && result.User != null)
                {
                    Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    _logger.LogInformation($"User logged in: {model.Email} with role {result.User.Role?.RoleName}");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToRoleDashboard(result.User.Role?.RoleName);
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                await _authService.RevokeTokenAsync(token);
            }

            Response.Cookies.Delete("AuthToken");

            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _auditService.LogActionAsync(userId, "LOGOUT", "User", userId, "User logged out", ipAddress);
            }

            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Access Denied & Lockout

        [HttpGet]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout() => View();

        #endregion

        #region Helper Methods

        private IActionResult RedirectToRoleDashboard(string? roleName = null)
        {
            roleName ??= User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return roleName switch
            {
                UserRoles.Admin => RedirectToAction("Dashboard", "Admin"),
                UserRoles.HeadOffice => RedirectToAction("Dashboard", "HeadOffice"),
                UserRoles.RdcStaff => RedirectToAction("Dashboard", "RdcStaff"),
                UserRoles.Logistics => RedirectToAction("Dashboard", "Logistics"),
                UserRoles.Driver => RedirectToAction("Dashboard", "Driver"),
                UserRoles.Finance => RedirectToAction("Dashboard", "Finance"),
                UserRoles.SalesRep => RedirectToAction("Dashboard", "SalesRep"),
                UserRoles.Customer => RedirectToAction("Dashboard", "Customer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        #endregion
    }
}