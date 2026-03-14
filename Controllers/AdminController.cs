using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISDN.Constants;
using ISDN.Services;
using ISDN.Repositories;
using ISDN.Data;
using ISDN.ViewModels;
using ISDN.Models;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;


namespace ISDN.Controllers
{
    /// <summary>
    /// Admin Dashboard Controller
    /// Manages users, roles, permissions, and views audit logs
    /// </summary>
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : BaseRdcController
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditService;
        private readonly IsdnDbContext _context;

        public AdminController(
            IUserRepository userRepository, 
            IAuditLogService auditService,
            IsdnDbContext context)
        {
            _userRepository = userRepository;
            _auditService = auditService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AdminRevenue()
        {
            var model = new ISDN.ViewModels.AdminRevenueViewModel();

            var userId = GetUserId();
            var userRdc = GetUserRdcId();

            // Payments query with Include(Order)
            var paymentsQuery = _context.Payments.Include(p => p.Order).AsQueryable();

            // If user is not ADMIN or Head Office (i.e., has RDC) apply RDC filter
            if (!(User.IsInRole(UserRoles.Admin) || IsHeadOfficeUser()))
            {
                paymentsQuery = ApplyRdcFilter(paymentsQuery);
            }

            // Total sales = sum of completed/paid payments
            model.TotalSales = await paymentsQuery
                .Where(p => p.PaymentStatus != null && (p.PaymentStatus.ToLower() == "completed" || p.PaymentStatus.ToLower() == "paid" || p.PaymentStatus.ToLower() == "successful"))
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            // Returns: join order_returns with order_items to compute returned value
            var returnsQuery = from r in _context.OrderReturns
                               join oi in _context.OrderItems on new { r.OrderId, r.ProductId } equals new { oi.OrderId, oi.ProductId }
                               join o in _context.Orders on r.OrderId equals o.OrderId
                               select new { Return = r, OrderItem = oi, Order = o };

            if (!(User.IsInRole(UserRoles.Admin) || IsHeadOfficeUser()))
            {
                // filter returns by RDC
                returnsQuery = returnsQuery.Where(x => x.Order.RdcId == userRdc);
            }

            var totalReturns = await returnsQuery
                .Select(x => (decimal)(x.OrderItem.Quantity == 0 ? 0 : (x.OrderItem.Subtotal / x.OrderItem.Quantity) * x.Return.Quantity))
                .SumAsync();

            model.TotalReturns = totalReturns;
            model.NetRevenue = model.TotalSales - model.TotalReturns;

            // Customer count
            var customersQuery = _context.Customers.AsQueryable();
            if (!(User.IsInRole(UserRoles.Admin) || IsHeadOfficeUser()))
            {
                customersQuery = customersQuery.Where(c => c.RdcId == userRdc);
            }
            model.CustomerCount = await customersQuery.CountAsync(c => c.IsActive);

            // Completed orders (DELIVERED)
            var ordersQuery = _context.Orders.AsQueryable();
            if (!(User.IsInRole(UserRoles.Admin) || IsHeadOfficeUser()))
            {
                ordersQuery = ordersQuery.Where(o => o.RdcId == userRdc);
            }
            model.CompletedOrders = await ordersQuery.CountAsync(o => o.Status != null && o.Status.ToUpper() == "DELIVERED");

            model.RdcId = userRdc ?? 0;
            if (userRdc.HasValue)
            {
                var rdc = await _context.Rdcs.FindAsync(userRdc.Value);
                if (rdc != null) model.RdcName = rdc.RdcName;
            }

            return View("~/Views/Admin/AdminRevenue.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var auditLogs = await _auditService.GetAuditLogsAsync();
                var totalRoles = await _context.Roles.CountAsync();
                var availableInventory = await _context.Inventories.CountAsync();

                ViewBag.TotalUsers = users?.Count() ?? 0;
                ViewBag.TotalRoles = totalRoles;
                ViewBag.AvailableInventory = availableInventory; 
                ViewBag.RecentLogs = auditLogs?.Take(10) ?? Enumerable.Empty<AuditLog>();
                
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading admin dashboard: {ex.Message}");
                ViewBag.TotalUsers = 0;
                ViewBag.TotalRoles = 0;
                ViewBag.AvailableInventory = 0;
                ViewBag.RecentLogs = Enumerable.Empty<AuditLog>();
                ViewBag.ErrorMessage = "Unable to load dashboard data. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var viewModels = users.Select(u => new UserManagementViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role?.RoleName ?? "N/A",
                RoleId = u.RoleId,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                TwoFactorEnabled = u.TwoFactorEnabled
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _context.Roles.ToListAsync();

            var viewModel = new AssignRoleViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                CurrentRole = user.Role?.RoleName ?? "N/A",
                AvailableRoles = roles.Select(r => r.RoleName).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _context.Roles.ToListAsync();
                model.AvailableRoles = roles.Select(r => r.RoleName).ToList();
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == model.SelectedRole);
            if (role == null)
            {
                ModelState.AddModelError("", "Invalid role selected");
                return View(model);
            }

            // Update user role
            user.RoleId = role.RoleId;
            await _context.SaveChangesAsync();

            // Log the action
            var adminId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            await _auditService.LogActionAsync(
                adminId,
                "ROLE_CHANGED",
                "User",
                user.UserId,
                $"Changed role of {user.Email} to {model.SelectedRole}",
                ipAddress
            );

            TempData["SuccessMessage"] = $"Role successfully changed to {model.SelectedRole}";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            // Log the action
            var adminId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            await _auditService.LogActionAsync(
                adminId,
                user.IsActive ? "USER_ACTIVATED" : "USER_DEACTIVATED",
                "User",
                user.UserId,
                $"{(user.IsActive ? "Activated" : "Deactivated")} user {user.Email}",
                ipAddress
            );

            TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _auditService.GetAuditLogsAsync();
            return View(logs);
        }
    }
}
