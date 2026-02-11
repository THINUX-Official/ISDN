using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Services;
using ISDN.ViewModels;

namespace ISDN.Controllers
{
    /// <summary>
    /// AccountController handles user authentication with JWT tokens.
    /// Implements:
    /// - JWT-based authentication
    /// - BCrypt password hashing
    /// - Cookie-based JWT storage for MVC views
    /// - Audit logging for all authentication events
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly IAuditLogService _auditService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthenticationService authService,
            IAuditLogService auditService,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
        }

        #region Register

        /// <summary>
        /// GET: /Account/Register
        /// Displays the registration form for new users.
        /// </summary>
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
        /// Creates a new user account with BCrypt password hashing.
        /// Default role: CUSTOMER
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterAsync(
                    model.FullName, 
                    model.Email, 
                    model.Password, 
                    model.RoleName);

                if (result.Success)
                {
                    _logger.LogInformation($"New user registered: {model.Email}");
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        #endregion

        #region Login

        /// <summary>
        /// GET: /Account/Login
        /// Displays the login form for all users (single login page).
        /// </summary>
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

        /// <summary>
        /// POST: /Account/Login
        /// Authenticates user and issues JWT token.
        /// Token stored in HTTP-only cookie for security.
        /// Redirects to role-specific dashboard.
        /// 
        /// JWT Token Claims:
        /// - user_id: User's unique ID from database
        /// - email: User's email address
        /// - role: User's role (ADMIN, DRIVER, RDC_STAFF, etc.)
        /// - rdc_id: User's RDC assignment (only for regional users, NOT included for Head Office)
        /// 
        /// Important: Users must re-login after database changes that affect their user record or RDC assignment
        /// to receive updated JWT tokens with correct claims.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Get client IP address for audit logging
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var result = await _authService.LoginAsync(model.Email, model.Password, ipAddress);

                if (result.Success && result.User != null)
                {
                    // Store JWT token in HTTP-only cookie (secure for MVC)
                    // Token contains claims: user_id, email, role, rdc_id (if assigned)
                    Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Set to true in production with HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    _logger.LogInformation($"User logged in: {model.Email} with role {result.User.Role?.RoleName}, RDC: {result.User.RdcId?.ToString() ?? "None (Head Office)"}");

                    // Redirect based on return URL or role
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

        /// <summary>
        /// POST: /Account/Logout
        /// Revokes JWT token and clears authentication cookie.
        /// </summary>
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

            // Clear authentication cookie
            Response.Cookies.Delete("AuthToken");

            // Log logout
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

        #region Access Denied

        /// <summary>
        /// GET: /Account/AccessDenied
        /// Displays when a user attempts to access a resource without proper authorization.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Lockout

        /// <summary>
        /// GET: /Account/Lockout
        /// Placeholder for account lockout page (can be implemented with failed login tracking).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Redirects user to appropriate dashboard based on their role.
        /// </summary>
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

