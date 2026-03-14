using ISDN.Constants;
using ISDN.Data;
using ISDN.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.Customer)]
    public class ProfileController : BaseRdcController
    {
        private readonly IsdnDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IsdnDbContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateProfile(CustomerProfileViewModel model)
        {
            var userId = GetUserId();
            if (userId == 0) return Json(new { success = false, error = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Json(new { success = false, error = string.IsNullOrEmpty(errors) ? "Invalid input." : errors });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (customer == null || user == null)
                    return Json(new { success = false, error = "Profile not found." });

                // Verify current password
                var currentPwd = model.CurrentPassword ?? string.Empty;
                var validPassword = false;
                try
                {
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        validPassword = BCrypt.Net.BCrypt.Verify(currentPwd, user.PasswordHash);
                    }
                    else if (!string.IsNullOrEmpty(customer.temp_password_hash))
                    {
                        validPassword = BCrypt.Net.BCrypt.Verify(currentPwd, customer.temp_password_hash);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Password verification failed for user {UserId}", userId);
                    validPassword = false;
                }

                if (!validPassword)
                {
                    return Json(new { success = false, error = "Invalid current password. If you forgot it, please reset via the login interface." });
                }

                // Apply changes
                customer.first_name = model.FirstName;
                customer.last_name = model.LastName;
                customer.phone_number = model.PhoneNumber;
                customer.email = model.Email;
                user.Email = model.Email;
                user.FullName = $"{model.FirstName} {model.LastName}";

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var newHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    customer.temp_password_hash = newHash;
                    user.PasswordHash = newHash;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, redirectUrl = Url.Action("Dashboard", "Customer") });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update profile for user {UserId}", userId);
                return Json(new { success = false, error = "An error occurred while saving your profile." });
            }
        }
    }
}