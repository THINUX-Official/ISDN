using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ISDN.Constants;
using ISDN.Data;
using ISDN.Models;
using ISDN.Services;
using ISDN.ViewModels;
using ISDN_Distribution.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISDN.Services.IAuthenticationService _authService;
        private readonly IAuditLogService _auditService;
        private readonly ILogger<AccountController> _logger;
        private readonly IsdnDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(
            ISDN.Services.IAuthenticationService authService,
            IAuditLogService auditService,
            ILogger<AccountController> logger,
            IsdnDbContext context,
            IEmailService emailService)
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }

        #region Registration Flow

        // Single Business Owner (SBO)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                string nic = HttpContext.Session.GetString("RegistrationNIC") ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(nic))
                {
                    bool nicExists = await _context.Customers.AnyAsync(c => c.city != null && c.city.EndsWith("|" + nic));
                    if (nicExists)
                    {
                        ModelState.AddModelError(string.Empty, "This NIC is already registered. One ID can only be used by one customer.");
                        return View(model);
                    }
                }

                string combinedHash = ISDN.Helpers.AuthHelper.CreateTempPasswordHash(model.Password, null);

                var customer = new Customer
                {
                    first_name = model.FirstName,
                    last_name = model.LastName,
                    email = model.Email,
                    phone_number = model.PhoneNumber,
                    business_name = ISDN.Helpers.AuthHelper.FormatBusinessName("", "SBO", model.BusinessName, "Main Branch"),
                    street_address = model.StreetAddress,
                    city = ISDN.Helpers.CityNicHelper.CombineCityAndNic(model.City, nic),
                    zip_code = model.ZipCode,
                    temp_password_hash = combinedHash,
                    registration_status = "PENDING",
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return RedirectToAction("RegistrationSuccess", new { code = "N/A" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SBO registration failed");
                ModelState.AddModelError(string.Empty, "Registration failed.");
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SBORegistrationSuccess()
        {
            return View();
        }

        // PBOS Single
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterPBOSingle()
        {
            var model = new RegisterPBOSingleViewModel
            {
                Branches = new List<BranchViewModel> { new BranchViewModel() }
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPBOSingle(RegisterPBOSingleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string nic = HttpContext.Session.GetString("RegistrationNIC") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(nic))
            {
                bool nicExists = await _context.Customers.AnyAsync(c => c.city != null && c.city.EndsWith("|" + nic));
                if (nicExists)
                {
                    ModelState.AddModelError(string.Empty, "This NIC is already registered. One ID can only be used by one customer.");
                    return View(model);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? uniqueCode = (model.RegistrationPreference == "Code")
                    ? new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6)
                        .Select(s => s[new Random().Next(s.Length)]).ToArray())
                    : null;

                string combinedHash = ISDN.Helpers.AuthHelper.CreateTempPasswordHash(model.Password, uniqueCode);

                bool isFirstBranch = true;
                foreach (var branch in model.Branches ?? new List<BranchViewModel>())
                {
                    var customerBranch = new Customer
                    {
                        first_name = model.FirstName,
                        last_name = model.LastName,
                        email = isFirstBranch ? model.Email : null,
                        phone_number = model.PhoneNumber,
                        business_name = ISDN.Helpers.AuthHelper.FormatBusinessName(model.BusinessType, "PBOS", model.BusinessName, branch.BranchName),
                        street_address = branch.StreetAddress,
                        city = ISDN.Helpers.CityNicHelper.CombineCityAndNic(branch.City, nic),
                        zip_code = branch.ZipCode,
                        temp_password_hash = combinedHash,
                        registration_status = "PENDING",
                        IsActive = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Customers.Add(customerBranch);
                    isFirstBranch = false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("RegistrationSuccess", new { code = uniqueCode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var databaseError = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Detailed Registration Error: {Error}", databaseError);
                ModelState.AddModelError(string.Empty, $"Database Error: {databaseError}");
                return View(model);
            }
        }

        // PBOM Multi
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterPBOMulti()
        {
            var model = new RegisterPBOMultiViewModel();
            var initialGroup = new BusinessTypeGroupViewModel();
            initialGroup.Branches.Add(new BranchViewModel());
            model.BusinessGroups.Add(initialGroup);
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPBOMulti(RegisterPBOMultiViewModel model)
        {
            if (model.BusinessGroups == null || model.BusinessGroups.Count < 2)
                ModelState.AddModelError(string.Empty, "You must register at least two different business types.");

            foreach (var group in model.BusinessGroups ?? new List<BusinessTypeGroupViewModel>())
            {
                if (group.Branches == null || group.Branches.Count < 1)
                    ModelState.AddModelError(string.Empty, $"Business type '{group.BusinessType}' must have at least one branch.");
            }

            if (!ModelState.IsValid) return View(model);

            string nic = HttpContext.Session.GetString("RegistrationNIC") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(nic))
            {
                bool nicExists = await _context.Customers.AnyAsync(c => c.city != null && c.city.EndsWith("|" + nic));
                if (nicExists)
                {
                    ModelState.AddModelError(string.Empty, "This NIC is already registered. One ID can only be used by one customer.");
                    return View(model);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? uniqueCode = (model.RegistrationPreference == "Code")
                    ? new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6)
                        .Select(s => s[new Random().Next(s.Length)]).ToArray())
                    : null;

                string combinedHash = ISDN.Helpers.AuthHelper.CreateTempPasswordHash(model.Password, uniqueCode);

                bool isFirstOverallBranch = true;
                foreach (var group in model.BusinessGroups)
                {
                    foreach (var branch in group.Branches)
                    {
                        var customerRecord = new Customer
                        {
                            first_name = model.FirstName,
                            last_name = model.LastName,
                            email = isFirstOverallBranch ? model.Email : null,
                            phone_number = model.PhoneNumber,
                            business_name = ISDN.Helpers.AuthHelper.FormatBusinessName(group.BusinessType, "PBOM", model.BusinessName, branch.BranchName),
                            street_address = branch.StreetAddress,
                            city = ISDN.Helpers.CityNicHelper.CombineCityAndNic(branch.City, nic),
                            zip_code = branch.ZipCode,
                            temp_password_hash = combinedHash,
                            registration_status = "PENDING",
                            IsActive = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Customers.Add(customerRecord);
                        isFirstOverallBranch = false;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("RegistrationSuccess", new { code = uniqueCode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Multi-Business Registration Error: {Message}", innerException);
                ModelState.AddModelError(string.Empty, "Database Error: " + innerException);
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegistrationSuccess(string? code)
        {
            ViewBag.GeneratedCode = code;
            return View();
        }

        #endregion

        #region Branch Manager Registration (BM)

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterBM(string code)
        {
            if (string.IsNullOrEmpty(code)) return RedirectToAction("Index", "Home");

            var allCustomers = await _context.Customers.ToListAsync();
            var branches = allCustomers.Where(c => c.GetRegistrationCode() == code).ToList();

            if (!branches.Any()) return RedirectToAction("Index", "Home");

            var model = new RegisterBMViewModel
            {
                InvitationCode = code,
                AvailableBranches = branches.Select(b => new BranchInfo
                {
                    CustomerId = b.CustomerId,
                    BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2) + (string.IsNullOrEmpty(ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)) ? "" : " - " + ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)),
                    BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                    City = b.city ?? string.Empty
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBM(RegisterBMViewModel model)
        {
            var allCustomers = await _context.Customers.ToListAsync();
            var branches = allCustomers.Where(c => c.GetRegistrationCode() == model.InvitationCode).ToList();

            if (!ModelState.IsValid)
            {
                model.AvailableBranches = branches.Select(b => new BranchInfo
                {
                    CustomerId = b.CustomerId,
                    BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2) + (string.IsNullOrEmpty(ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)) ? "" : " - " + ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)),
                    BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                    City = b.city ?? string.Empty
                }).ToList();
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var branchCustomer = branches.FirstOrDefault(b => b.CustomerId == model.SelectedBranchId);
                if (branchCustomer == null)
                {
                    ModelState.AddModelError(string.Empty, "Selected branch is invalid.");
                    model.AvailableBranches = branches.Select(b => new BranchInfo
                    {
                        CustomerId = b.CustomerId,
                        BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2),
                        BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                        City = b.city ?? string.Empty
                    }).ToList();
                    return View(model);
                }

                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == UserRoles.Customer);
                if (customerRole == null) throw new Exception("Customer role not found in system.");

                var user = new User
                {
                    FullName = $"{model.FirstName} {model.LastName}",
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RoleId = customerRole.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                branchCustomer.UserId = user.UserId;
                branchCustomer.IsActive = true;
                branchCustomer.registration_status = "APPROVED";
                if (string.IsNullOrEmpty(branchCustomer.email)) branchCustomer.email = model.Email;

                _context.Customers.Update(branchCustomer);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("SBORegistrationSuccess");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to register: " + ex.Message);
                model.AvailableBranches = branches.Select(b => new BranchInfo
                {
                    CustomerId = b.CustomerId,
                    BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2),
                    BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                    City = b.city ?? string.Empty
                }).ToList();
                return View(model);
            }
        }

        #endregion

        #region Password Reset Flow

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // To prevent email enumeration, always show success message.
            TempData["SuccessMessage"] = "If your email is registered as a customer, a reset link has been sent.";

            // Build token valid for 30 minutes
            long expiryTime = DateTime.UtcNow.AddMinutes(30).Ticks;
            string rawToken = $"{model.Email}|{expiryTime}";
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));

            var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = model.Email }, Request.Scheme);

            _logger.LogWarning("Generated Reset Link for {Email}: {Link}", model.Email, resetLink);

            string emailSubject = "ISDN Distribution - Password Reset Request";
            string emailBody = $@"<h3>Password Reset Request</h3>
                <p>Hello,</p>
                <p>We received a request to reset your password. You can reset your password by clicking the link below:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 30 minutes.</p>
                <p>If you did not request a password reset, please ignore this email.</p>
                <br /><p>Thank you,</p><p>ISDN Distribution Team</p>";

            try
            {
                await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
                _logger.LogInformation("Password reset email sent to: {Email}", model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to: {Email}. Reset Link: {Link}", model.Email, resetLink);
                // For dev, keep link visible in logs
                Console.WriteLine("Password reset link: " + resetLink);
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Validate token
            try
            {
                string rawToken = Encoding.UTF8.GetString(Convert.FromBase64String(model.Token));
                var parts = rawToken.Split('|');
                if (parts.Length != 2 || parts[0] != model.Email)
                {
                    TempData["ErrorMessage"] = "Invalid token.";
                    return View(model);
                }

                long expiryTicks = long.Parse(parts[1]);
                if (DateTime.UtcNow.Ticks > expiryTicks)
                {
                    TempData["ErrorMessage"] = "Token has expired. Please request a new password reset link.";
                    return View(model);
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Invalid token format.";
                return View(model);
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return View(model);
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.PasswordHash = hashedPassword;

                var customers = await _context.Customers.Where(c => c.UserId == user.UserId).ToListAsync();
                foreach (var customer in customers)
                {
                    customer.temp_password_hash = hashedPassword;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password reset successfully. You can now login.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user email: {Email}", model.Email);
                TempData["ErrorMessage"] = "An error occurred while resetting the password.";
                return View(model);
            }
        }

        #endregion

        #region Login / Logout

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToRoleDashboard();
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

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                        new Claim(ClaimTypes.Role, result.User.Role?.RoleName ?? "Driver")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                    await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToRoleDashboard(result.User.Role?.RoleName);
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

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
            await HttpContext.SignOutAsync("CookieAuth");

            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Helpers

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private IActionResult RedirectToRoleDashboard(string? roleName = null)
        {
            roleName ??= User.FindFirst(ClaimTypes.Role)?.Value;

            return roleName switch
            {
                UserRoles.Admin => RedirectToAction("Dashboard", "Admin"),
                UserRoles.Customer => RedirectToAction("Dashboard", "Customer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        #endregion
    }
}