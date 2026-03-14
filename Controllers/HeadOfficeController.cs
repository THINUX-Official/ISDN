using ISDN.Constants;
using ISDN.Data;
using ISDN.Helpers;
using ISDN.Models;
using ISDN_Distribution.Models;
using ISDN_Distribution.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISDN.Models.ViewModels; 

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
            // 1. Fetch all customers from the database
            var allCustomers = await _context.Customers.ToListAsync();

            // 2. Map the data to the specific lists the SBO UI expects.
            // Your .cshtml uses ViewBag.PendingCustomers, ViewBag.ActiveCustomers, 
            // and ViewBag.DisapprovedCustomers.

            ViewBag.PendingCustomers = allCustomers
                .Where(c => c.registration_status == "PENDING")
                .ToList();

            ViewBag.ActiveCustomers = allCustomers
                .Where(c => c.registration_status == "APPROVED")
                .ToList();

            ViewBag.DisapprovedCustomers = allCustomers
                .Where(c => c.registration_status == "DISAPPROVED")
                .ToList();

            // 3. Keep the RDC list for your dropdowns
            ViewBag.Rdcs = await _context.Rdcs.ToListAsync();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCluster(string uniqueCode, Dictionary<int, int> branchRdcAssignments)
        {
            // 1. Fetch only pending customers from DB (this is the SQL part)
            var allPending = await _context.Customers
                .Where(c => c.registration_status == "PENDING")
                .ToListAsync();

            // 2. Filter the retrieved list in memory (this is the C# part)
            // Now that the data is in memory, GetRegistrationCode() works perfectly.
            var branches = allPending
                .Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode)
                .ToList();

            if (!branches.Any())
            {
                TempData["Error"] = "No pending customers found for this cluster.";
                return RedirectToAction(nameof(CustomerManagement));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var branch in branches)
                {
                    if (branchRdcAssignments.TryGetValue(branch.CustomerId, out int rdcId))
                    {
                        // Logic for the 'Main Branch'
                        if (branch.business_name != null && branch.business_name.Contains("Main Branch"))
                        {
                            var newUser = new User
                            {
                                FullName = $"{branch.first_name} {branch.last_name}",
                                Email = branch.email,
                                PasswordHash = branch.GetPasswordHash(), // Use your model helper
                                RoleId = 8,
                                RdcId = rdcId,
                                IsActive = true
                            };
                            _context.Users.Add(newUser);
                            await _context.SaveChangesAsync();
                            branch.UserId = newUser.UserId;
                        }

                        branch.RdcId = rdcId;
                        branch.registration_status = "APPROVED";
                        branch.IsActive = true;
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = "Business cluster approved successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Approval failed: " + ex.Message;
            }
            return RedirectToAction(nameof(CustomerManagement));
        }




        [HttpGet]
        public async Task<IActionResult> ClusterManagement()
        {
            var allCustomers = await _context.Customers.ToListAsync();

            // Group everything by UniqueCode
            var clusters = allCustomers
                .Where(c => c.registration_status == "PENDING" || c.registration_status == "APPROVED")
                .GroupBy(c => c.GetRegistrationCode() ?? "SBO_" + c.CustomerId)
                .Select(g => new CustomerClusterViewModel
                {
                    UniqueCode = g.Key,
                    BusinessName = AuthHelper.GetValue(g.FirstOrDefault().business_name, 3),
                    BusinessType = AuthHelper.GetValue(g.FirstOrDefault().business_name, 2),
                    Email = g.FirstOrDefault().email,
                    Branches = g.Select(b => new CustomerBranchViewModel
                    {
                        CustomerId = b.CustomerId,
                        BranchName = AuthHelper.GetValue(b.business_name, 4),
                        City = b.city,
                        Status = b.registration_status,
                        IsMainBranch = b.business_name.Contains("Main Branch")
                    }).ToList()
                }).ToList();

            // Split for the toggle: PBOS (Count == 1) vs PBOM (Count > 1)
            ViewBag.PBOS = clusters.Where(c => c.Branches.Count == 1).ToList();
            ViewBag.PBOM = clusters.Where(c => c.Branches.Count > 1).ToList();
            ViewBag.Rdcs = await _context.Rdcs.ToListAsync();

            return View();
        }




        [HttpPost]
        public async Task<IActionResult> ManageClusterState(string uniqueCode, string action, Dictionary<int, int> rdcAssignments)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var branches = await _context.Customers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode).ToListAsync();

                foreach (var b in branches)
                {
                    if (action == "APPROVE")
                    {
                        b.registration_status = "APPROVED";
                        b.IsActive = true;
                        if (rdcAssignments.TryGetValue(b.CustomerId, out int rdc)) b.RdcId = rdc;
                    }
                    else if (action == "SUSPEND") b.IsActive = false;
                    else if (action == "DELETE") _context.Customers.Remove(b);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch { await transaction.RollbackAsync(); return BadRequest(); }
        }


        // POST: /HeadOffice/ApproveCustomer
        // ISDN.Controllers/HeadOfficeController.cs

        // 1. Approve Logic එකේ වෙනස
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCustomer(int customerId, int rdcId)
        {
            if (customerId <= 0 || rdcId <= 0) return RedirectToAction(nameof(CustomerManagement));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Find the target branch and its cluster
                var targetCustomer = await _context.Customers.FindAsync(customerId);
                if (targetCustomer == null) throw new Exception("Customer not found.");

                string uniqueCode = targetCustomer.GetRegistrationCode() ?? "SBO_" + targetCustomer.CustomerId;

                // 2. Fetch all members of this cluster
                var allPending = await _context.Customers.Where(c => c.registration_status == "PENDING").ToListAsync();
                var clusterBranches = allPending
                    .Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode)
                    .ToList();

                // 3. Process the cluster
                bool userCreated = false;
                foreach (var branch in clusterBranches)
                {
                    // Only create one user for the cluster (using the Main Branch)
                    if (!userCreated && branch.business_name != null && branch.business_name.Contains("Main Branch"))
                    {
                        var newUser = new User
                        {
                            FullName = $"{branch.first_name} {branch.last_name}",
                            Email = branch.email,
                            PasswordHash = branch.GetPasswordHash(),
                            RoleId = 8,
                            RdcId = rdcId,
                            IsActive = true
                        };
                        _context.Users.Add(newUser);
                        await _context.SaveChangesAsync();
                        branch.UserId = newUser.UserId;
                        userCreated = true;
                    }

                    // Update status for every branch in the cluster
                    branch.RdcId = rdcId;
                    branch.registration_status = "APPROVED";
                    branch.IsActive = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = "Business cluster approved successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Approval failed: {ex.Message}";
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


        private bool IsSboCustomer(string? businessName)
        {
            // Adjust these keywords to match exactly how your registration logic stores them
            if (string.IsNullOrEmpty(businessName)) return false;
            return businessName.Contains("SBO", StringComparison.OrdinalIgnoreCase);
        }
    }
}
