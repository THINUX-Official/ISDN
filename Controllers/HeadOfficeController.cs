using ISDN.Constants;
using ISDN.Data;
using ISDN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

namespace ISDN.Controllers
{
    /// <summary>
    /// Head Office Dashboard Controller
    /// Views reports, KPIs, and manages high-level operations
    /// Head Office users have access to ALL RDC data (rdc_id = NULL)
    /// </summary>
    [Authorize(Roles = UserRoles.HeadOffice)]
    public class HeadOfficeController : BaseRdcController
    {
        private readonly IsdnDbContext _context;

        public HeadOfficeController(IsdnDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.IsHeadOffice = IsHeadOfficeUser();
            ViewBag.RdcId = GetUserRdcId();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            // Head Office sees all orders across all RDCs
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Apply RDC filter (will return all for Head Office)
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var totalOrders = await ordersQuery.CountAsync();
            var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.IsHeadOffice = IsHeadOfficeUser();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> KPIs()
        {
            // Head Office can view KPIs across all RDCs
            var ordersQuery = _context.Orders.AsQueryable();
            ordersQuery = ApplyRdcFilter(ordersQuery);

            var deliveriesQuery = _context.Deliveries.AsQueryable();
            deliveriesQuery = ApplyRdcFilter(deliveriesQuery);

            var paymentsQuery = _context.Payments.AsQueryable();
            paymentsQuery = ApplyRdcFilter(paymentsQuery);

            ViewBag.TotalOrders = await ordersQuery.CountAsync();
            ViewBag.PendingDeliveries = await deliveriesQuery.CountAsync(d => d.Status == "Pending");
            ViewBag.CompletedPayments = await paymentsQuery.CountAsync(p => p.PaymentStatus == "Completed");
            ViewBag.IsHeadOffice = IsHeadOfficeUser();

            return View();
        }

        // GET: /HeadOffice/CustomerManagement
        [HttpGet]
        public async Task<IActionResult> CustomerManagement()
        {
            var pending = await _context.Customers.Where(c => c.registration_status == "PENDING").ToListAsync();
            var active = await _context.Customers.Where(c => c.registration_status == "APPROVED" && c.IsActive).ToListAsync();
            var disapproved = await _context.Customers.Where(c => c.registration_status == "DISAPPROVED").ToListAsync();

            ViewBag.PendingCustomers = pending;
            ViewBag.ActiveCustomers = active;
            ViewBag.DisapprovedCustomers = disapproved;
            ViewBag.Rdcs = await _context.Rdcs.ToListAsync(); // RDC Dropdown එක සඳහා

            return View();
        }

        // POST: /HeadOffice/ApproveCustomer
        // ISDN.Controllers/HeadOfficeController.cs

        // 1. Approve Logic එකේ වෙනස
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCustomer(int customerId, int rdcId)
        {
            // Debugging වලට ලේසි වෙන්න values check කරමු
            if (customerId <= 0 || rdcId <= 0)
            {
                TempData["Error"] = $"Invalid Selection: CustomerID={customerId}, RdcID={rdcId}";
                return RedirectToAction(nameof(CustomerManagement));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Customer ව හොයාගන්න
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found in database.";
                    return RedirectToAction(nameof(CustomerManagement));
                }

                // 2. User record එක හදන්න කලින් දත්ත තියෙනවාද බලන්න
                if (string.IsNullOrEmpty(customer.email))
                {
                    throw new Exception("Customer email is missing. Cannot create a user account.");
                }

                var newUser = new User
                {
                    // Database එකේ තියෙන Column නම full_name නම්, මෙතන FullName කියලා දාන්න (ඔයාගේ Model එකේ තියෙන විදිහට)
                    FullName = $"{customer.first_name} {customer.last_name}",
                    Email = customer.email,
                    // මෙන්න මෙතනයි වැදගත්ම දේ: Database එකේ password_hash නම්, C# Model එකේ ඒක මොකක්ද බලන්න.
                    // බොහෝ විට ඒක PasswordHash වෙන්න ඇති.
                    PasswordHash = customer.temp_password_hash ?? "Temporary123!",
                    RoleId = 8,
                    RdcId = rdcId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // මෙතන Customer Update කරන කොටස
                customer.UserId = newUser.UserId; // මෙතන 'UserId' ද 'user_id' ද කියලා Model එකේ බලන්න
                customer.RdcId = rdcId;
                customer.registration_status = "APPROVED";
                customer.IsActive = true;

                _context.Entry(customer).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["Success"] = "Customer approved successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // මෙතනදී තමයි ඇත්තම ලෙඩේ බලාගන්න පුළුවන් වෙන්නේ
                string innerError = ex.InnerException != null ? ex.InnerException.Message : "";
                TempData["Error"] = $"Approval failed: {ex.Message}. Details: {innerError}";
            }
            return RedirectToAction(nameof(CustomerManagement));
        }

        // 2. Disapprove Logic එකේ වෙනස (Damith ගේ ප්‍රශ්නය Fix එක)
        [HttpPost]
        public async Task<IActionResult> DisapproveCustomer(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound();

            // මචං, මෙතන Remove කරන්න එපා. Status එක වෙනස් කරන්න විතරක්.
            // එතකොට තමයි එයා Disapproved tab එකට වැටෙන්නේ.
            customer.registration_status = "DISAPPROVED";
            customer.IsActive = false;
            customer.DisapprovedAt = DateTime.Now;

            // පරණ User account එකක් තිබුණොත් ඒක මකන්න
            if (customer.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(customer.UserId);
                if (user != null) _context.Users.Remove(user);
                customer.UserId = null;
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer moved to Disapproved tab.";
            return RedirectToAction(nameof(CustomerManagement));
        }

        // POST: /HeadOffice/PermanentDeleteCustomer
        // --- මේ කොටස Controller එකේ අදාළ තැන්වලට Replace කරන්න ---

        // 1. Permanent Delete එකේදී User වත් මකා දැමීම (syncing deletion)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteCustomer(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer != null && customer.registration_status == "DISAPPROVED")
                {
                    // පාරිභෝගිකයාට සම්බන්ධ User කෙනෙක් ඉන්නවා නම් එයාවත් මකනවා
                    if (customer.UserId.HasValue)
                    {
                        var user = await _context.Users.FindAsync(customer.UserId.Value);
                        if (user != null) _context.Users.Remove(user);
                    }

                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Customer and login account permanently removed.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete: This customer has transaction records (Orders/Payments).";
            }
            return RedirectToAction(nameof(CustomerManagement));
        }

        // 2. අලුත් Update Details Action එක (Popup එකෙන් එන දත්ත සඳහා)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomerDetails(int customerId, string businessName, string streetAddress, string city, string zipCode, int rdcId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null) return NotFound();

                customer.business_name = businessName;
                customer.street_address = streetAddress;
                customer.city = city;
                customer.zip_code = zipCode;
                customer.RdcId = rdcId;

                // User table එකේ තියෙන RdcId එකත් update කරන්න ඕනේ නම්:
                if (customer.UserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(customer.UserId.Value);
                    if (user != null) user.RdcId = rdcId;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer details updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Update failed: " + ex.Message;
            }
            return RedirectToAction(nameof(CustomerManagement));
        }
    }
}
