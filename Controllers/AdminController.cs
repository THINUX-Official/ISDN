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
    public class AdminController : Controller
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
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var auditLogs = await _auditService.GetAuditLogsAsync();
                
                ViewBag.TotalUsers = users?.Count() ?? 0;
                ViewBag.RecentLogs = auditLogs?.Take(10) ?? Enumerable.Empty<AuditLog>();
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.TotalUsers = 0;
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
