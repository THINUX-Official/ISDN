using ISDN.Constants;
using ISDN.Data;
using ISDN.Models;
using ISDN.Services;
using ISDN.ViewModels;
using ISDN_Distribution.Models;
using ISDN_Distribution.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq; // Added for string formatting logic

namespace ISDN.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISDN.Services.IAuthenticationService _authService;
        private readonly IAuditLogService _auditService;
        private readonly ILogger<AccountController> _logger;
        private readonly IsdnDbContext _context;

        public AccountController(
            ISDN.Services.IAuthenticationService authService,
            IAuditLogService auditService,
            ILogger<AccountController> logger,
            IsdnDbContext context)
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
            _context = context;
        }

        #region Registration Flow


        #region Single Business Owner Registration (SBO)
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
                // SBOs don't generate a code, so we pass null
                string combinedHash = ISDN.Helpers.AuthHelper.CreateTempPasswordHash(model.Password, null);

                var customer = new Customer
                {
                    first_name = model.FirstName,
                    last_name = model.LastName,
                    email = model.Email,
                    phone_number = model.PhoneNumber,
                    // Format: "[BusinessType] [UserType] BusinessName - BranchName"
                    // For SBO, BusinessType is empty, UserType is SBO
                   
                    business_name = ISDN.Helpers.AuthHelper.FormatBusinessName("", "SBO", model.BusinessName, "Main Branch"),
                    street_address = model.StreetAddress,
                    city = model.City,
                    zip_code = model.ZipCode,
                    temp_password_hash = combinedHash,
                    registration_status = "PENDING",
                    IsActive = false
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return RedirectToAction("SBORegistrationSuccess");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Registration failed.");
                return View(model);
            }
        }
        #endregion


        [HttpGet]
        [AllowAnonymous]
        public IActionResult SBORegistrationSuccess()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterPBOSingle()
        {
            // Initialize the model to ensure the Branches list isn't null
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? uniqueCode = (model.RegistrationPreference == "Code")
                    ? new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6)
                        .Select(s => s[new Random().Next(s.Length)]).ToArray())
                    : null;

                string combinedHash = ISDN.Helpers.AuthHelper.CreateTempPasswordHash(model.Password, uniqueCode);

                bool isFirstBranch = true;
                foreach (var branch in model.Branches)
                {
                    var customerBranch = new Customer
                    {
                        first_name = model.FirstName,
                        last_name = model.LastName,
                        // First branch gets the email, others null if preferred. 
                        // But since user will use code to register later, they will set their own email.
                        email = isFirstBranch ? model.Email : null,
                        phone_number = model.PhoneNumber,
                        // Use the BusinessType provided in the view model when formatting stored business_name
                        business_name = ISDN.Helpers.AuthHelper.FormatBusinessName(model.BusinessType, "PBOS", model.BusinessName, branch.BranchName),
                        street_address = branch.StreetAddress,
                        city = branch.City,
                        zip_code = branch.ZipCode,
                        temp_password_hash = combinedHash,
                        registration_status = "PENDING",
                        IsActive = false
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterPBOMulti()
        {
            var model = new RegisterPBOMultiViewModel();
            // Start with one business group and one branch by default
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

            foreach (var group in model.BusinessGroups ?? new())
            {
                if (group.Branches == null || group.Branches.Count < 1)
                    ModelState.AddModelError(string.Empty, $"Business type '{group.BusinessType}' must have at least one branch.");
            }

            if (!ModelState.IsValid) return View(model);

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
                            // Updated using helper
                            business_name = ISDN.Helpers.AuthHelper.FormatBusinessName(group.BusinessType, "PBOM", model.BusinessName, branch.BranchName),
                            street_address = branch.StreetAddress,
                            city = branch.City,
                            zip_code = branch.ZipCode,
                            temp_password_hash = combinedHash,
                            registration_status = "PENDING",
                            IsActive = false,
                            CreatedAt = DateTime.Now
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

        #region Branch Manager Registration (BM)

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterBM(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var allCustomers = await _context.Customers.ToListAsync();
            var branches = allCustomers.Where(c => c.GetRegistrationCode() == code).ToList();

            if (!branches.Any())
            {
                // Code not found
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterBMViewModel
            {
                InvitationCode = code,
                AvailableBranches = branches.Select(b => new BranchInfo
                {
                    CustomerId = b.CustomerId,
                    BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2) + 
                        (string.IsNullOrEmpty(ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)) ? "" : " - " + ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)),
                    BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                    City = b.city ?? ""
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
                    BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2) + 
                        (string.IsNullOrEmpty(ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)) ? "" : " - " + ISDN.Helpers.AuthHelper.GetValue(b.business_name, 3)),
                    BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                    City = b.city ?? ""
                }).ToList();
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var branchCustomer = branches.FirstOrDefault(b => b.CustomerId == model.SelectedBranchId);
                if (branchCustomer == null)
                {
                    ModelState.AddModelError("", "Selected branch is invalid.");
                    model.AvailableBranches = branches.Select(b => new BranchInfo
                    {
                        CustomerId = b.CustomerId,
                        BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2),
                        BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                        City = b.city ?? ""
                    }).ToList();
                    return View(model);
                }

                // Unlike PBO/SBO that requires admin approval before creating User, 
                // Branch Managers are immediately approved and Users created directly.
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

                // Link the User to the Customer branch and activate
                branchCustomer.UserId = user.UserId;
                branchCustomer.IsActive = true;
                branchCustomer.registration_status = "APPROVED";
                
                // If branch email was empty, update it
                if (string.IsNullOrEmpty(branchCustomer.email))
                {
                    branchCustomer.email = model.Email;
                }

                _context.Customers.Update(branchCustomer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("SBORegistrationSuccess");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Failed to register: " + ex.Message);
                model.AvailableBranches = branches.Select(b => new BranchInfo
                {
                    CustomerId = b.CustomerId,
                    BusinessName = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 2),
                    BusinessType = ISDN.Helpers.AuthHelper.GetValue(b.business_name, 1),
                    City = b.city ?? ""
                }).ToList();
                return View(model);
            }
        }

        #endregion

        #endregion

        #region Login / Logout

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

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                        new Claim(ClaimTypes.Role, result.User.Role?.RoleName ?? "Driver")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                    await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(
                        this.HttpContext,
                        "CookieAuth",
                        new ClaimsPrincipal(claimsIdentity));

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
            await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(this.HttpContext, "CookieAuth");

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