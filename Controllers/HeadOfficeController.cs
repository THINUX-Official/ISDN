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
        private readonly IConfiguration _config;

        public HeadOfficeController(IsdnDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> PreviewClusterEmail([FromBody] ClusterEmailRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrEmpty(req.UniqueCode))
                    return BadRequest(new { errors = new[] { "Invalid request payload." } });
                // req is model-bound from JSON body
                var all = await _context.Customers.ToListAsync();
                var clusterBranches = all.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == req.UniqueCode).ToList();
                var businessName = ISDN.Helpers.AuthHelper.GetValue(clusterBranches.FirstOrDefault()?.business_name, 3) ?? "Customer";
                var mainBranch = clusterBranches.FirstOrDefault(b => !string.IsNullOrEmpty(b.email));
                var mainEmail = mainBranch?.email ?? req.To;

                var branchNames = clusterBranches.Where(b => req.BranchIds == null || !req.BranchIds.Any() || req.BranchIds.Contains(b.CustomerId))
                    .Select(b => ISDN.Helpers.AuthHelper.GetValue(b.business_name, 4)).Where(n => !string.IsNullOrEmpty(n)).ToList();

                var branchNamesHtml = string.Join("", branchNames.Select(n => $"<li>{System.Net.WebUtility.HtmlEncode(n)}</li>"));

                string template;
                if (string.Equals(req.Type, "approve", StringComparison.OrdinalIgnoreCase))
                {
                    template = $"<p>Dear {System.Net.WebUtility.HtmlEncode(businessName)},</p>" +
                               "<p>Your registration has been <strong>approved</strong>. You can log in using the email <strong>{Email}</strong> and the password you set during registration.</p>" +
                               "<p>{Notes}</p>" +
                               "<p>Regards,<br/>{AdminName}</p>";
                }
                else if (string.Equals(req.Type, "suspend", StringComparison.OrdinalIgnoreCase))
                {
                    template = $"<p>Dear {System.Net.WebUtility.HtmlEncode(businessName)},</p>" +
                               "<p>The following branch(es) have been <strong>suspended</strong>:</p>" +
                               "<ul>" + branchNamesHtml + "</ul>" +
                               "<p>{Notes}</p>" +
                               "<p>Regards,<br/>{AdminName}</p>";
                }
                else
                {
                    template = $"<p>Dear {System.Net.WebUtility.HtmlEncode(businessName)},</p>" +
                               "<p>Your registration cluster (Code: <strong>{UniqueCode}</strong>) has been <strong>permanently deleted</strong>.</p>" +
                               "<p>{Notes}</p>" +
                               "<p>Regards,<br/>{AdminName}</p>";
                }

                var populated = template
                    .Replace("{Email}", System.Net.WebUtility.HtmlEncode(mainEmail ?? ""))
                    .Replace("{BranchNames}", string.Join(", ", branchNames.Select(System.Net.WebUtility.HtmlEncode)))
                    .Replace("{UniqueCode}", System.Net.WebUtility.HtmlEncode(req.UniqueCode ?? ""))
                    .Replace("{AdminName}", System.Net.WebUtility.HtmlEncode(req.From ?? "ISDN Head Office"))
                    .Replace("{Notes}", System.Net.WebUtility.HtmlEncode(req.Message ?? ""));

                return Ok(new { Html = populated });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            try
            {
                var activeCustomerCount = await _context.Customers.Where(c => c.IsActive && c.registration_status == "APPROVED").CountAsync();
                
                var currentMonthOrderCount = await _context.Orders.Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt < startOfNextMonth).CountAsync();
                
                var currentMonthSoldItemCount = await _context.OrderItems.Where(oi => oi.Order.CreatedAt >= startOfMonth && oi.Order.CreatedAt < startOfNextMonth)
                    .SumAsync(oi => (int?)oi.Quantity) ?? 0;

                var monthlySales = await _context.Orders.Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt < startOfNextMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

                var returnValue = await (
                    from r in _context.OrderReturns
                    join oi in _context.OrderItems
                        on new { r.OrderId, r.ProductId }
                        equals new { oi.OrderId, oi.ProductId }
                    join o in _context.Orders
                        on r.OrderId equals o.OrderId
                    where o.CreatedAt >= startOfMonth && o.CreatedAt < startOfNextMonth
                    select (decimal)((oi.Subtotal / oi.Quantity) * r.Quantity)
                ).SumAsync();

                var netRevenue = monthlySales - returnValue;

                ViewBag.ActiveCustomerCount = activeCustomerCount;
                ViewBag.CurrentMonthOrderCount = currentMonthOrderCount;
                ViewBag.CurrentMonthSoldItemCount = currentMonthSoldItemCount;
                ViewBag.CurrentMonthRevenue = netRevenue; 
                return View();
            }
            catch (Exception ex) {
                ViewBag.ActiveCustomerCount = 0;
                ViewBag.CurrentMonthOrderCount = 0;
                ViewBag.CurrentMonthSoldItemCount = 0;
                ViewBag.CurrentMonthRevenue = 0; 
                return View();
            }
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
            // Only load customers that are registered as SBO user type.
            // We first narrow results at the database level by checking the stored
            // business_name contains the pipe-delimited SBO marker ("|SBO|") to
            // avoid pulling every customer. Then we defensively filter in memory
            // using the AuthHelper to ensure we only include exact SBO user types.

            var dbCandidates = await _context.Customers.ToListAsync();
            dbCandidates = dbCandidates.Where(c => !string.IsNullOrEmpty(c.business_name) && c.business_name.Contains("|SBO|")).ToList();

            // The stored format is: |BusinessType|UserType|BusinessName|BranchName
            // UserType is at index 2 when using AuthHelper.GetValue
            var sboCustomers = dbCandidates
                .Where(c => ISDN.Helpers.AuthHelper.GetValue(c.business_name, 2)
                             .Equals("SBO", StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.PendingCustomers = sboCustomers.Where(c => c.registration_status == "PENDING").ToList();
            ViewBag.ActiveCustomers = sboCustomers.Where(c => c.registration_status == "APPROVED").ToList();
            ViewBag.DisapprovedCustomers = sboCustomers.Where(c => c.registration_status == "DISAPPROVED").ToList();

            // Keep the RDC list for your dropdowns
            ViewBag.Rdcs = await _context.Rdcs.ToListAsync();

            return View();
        }


        public class ApproveClusterRequest
        {
            public string? UniqueCode { get; set; }
            public Dictionary<int, int>? BranchRdcAssignments { get; set; }
        }

        public class ManageClusterRequest
        {
            public string? UniqueCode { get; set; }
            public string? Action { get; set; }
            public Dictionary<int, int>? RdcAssignments { get; set; }
            public List<int>? BranchIds { get; set; }
        }

        public class ManageBranchRequest
        {
            public int CustomerId { get; set; }
            public string? Action { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCluster([FromBody] ApproveClusterRequest? request)
        {
            if (request == null || string.IsNullOrEmpty(request.UniqueCode) || request.BranchRdcAssignments == null)
                return BadRequest("Invalid request payload.");

            var uniqueCode = request.UniqueCode;
            var branchRdcAssignments = request.BranchRdcAssignments;

            // 1. Fetch branches that belong to the cluster (by unique code)
            var allCustomers = await _context.Customers.ToListAsync();
            var branches = allCustomers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode).ToList();

            if (!branches.Any()) return BadRequest("No customers found for this cluster.");

            // Ensure RDC assignments are provided for every branch in the cluster
            var missingAssignments = branches.Where(b => !branchRdcAssignments.ContainsKey(b.CustomerId)).ToList();
            if (missingAssignments.Any())
            {
                return BadRequest("Please select an RDC for every branch before approving the cluster.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create a user only for the main branch (first branch that contains email)
                var mainBranch = branches.FirstOrDefault(b => !string.IsNullOrEmpty(b.email));
                User? createdUser = null;

                if (mainBranch != null && !mainBranch.UserId.HasValue)
                {
                    var passwordHash = mainBranch.GetPasswordHash();

                    var bManagerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "B_MANAGER");
                    if (bManagerRole == null)
                    {
                        bManagerRole = new Role { RoleName = "B_MANAGER" };
                        _context.Roles.Add(bManagerRole);
                        await _context.SaveChangesAsync();
                    }

                    var newUser = new User
                    {
                        FullName = $"{mainBranch.first_name} {mainBranch.last_name}",
                        Email = mainBranch.email?.ToLower().Trim() ?? string.Empty,
                        RoleId = bManagerRole.RoleId,
                        PasswordHash = passwordHash,
                        IsActive = true
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    createdUser = newUser;
                    mainBranch.UserId = newUser.UserId;
                }

                // Update every branch with selected RDC and approved state
                foreach (var branch in branches)
                {
                    if (branchRdcAssignments.TryGetValue(branch.CustomerId, out int rdcId))
                    {
                        branch.RdcId = rdcId;
                    }
                    branch.registration_status = "APPROVED";
                    branch.IsActive = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Approval failed: " + ex.Message);
            }
        }




        [HttpGet]
        public async Task<IActionResult> ClusterManagement()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPbosClusters(string tab)
        {
            var pbosClusters = await GetFilteredClustersAsync("PBOS", tab);
            ViewBag.Rdcs = await _context.Rdcs.ToListAsync();
            ViewData["Type"] = "pbos";
            ViewData["Tab"] = tab;
            return PartialView("_ClusterTab", pbosClusters);
        }

        [HttpGet]
        public async Task<IActionResult> GetPbomClusters(string tab)
        {
            var pbomClusters = await GetFilteredClustersAsync("PBOM", tab);
            ViewBag.Rdcs = await _context.Rdcs.ToListAsync();
            ViewData["Type"] = "pbom";
            ViewData["Tab"] = tab;
            return PartialView("_ClusterTab", pbomClusters);
        }

        private async Task<List<CustomerClusterViewModel>> GetFilteredClustersAsync(string requestedType, string tab)
        {
            var allCustomers = await _context.Customers.ToListAsync();

            var clusters = allCustomers
                .GroupBy(c => c.GetRegistrationCode() ?? "SBO_" + c.CustomerId)
                .Select(g => new CustomerClusterViewModel
                {
                    UniqueCode = g.Key,
                    BusinessName = (AuthHelper.GetValue(g.FirstOrDefault().business_name, 3) ?? string.Empty).Trim(),
                    BusinessType = (AuthHelper.GetValue(g.FirstOrDefault().business_name, 1) ?? string.Empty).Trim(),
                    UserType = (AuthHelper.GetValue(g.FirstOrDefault().business_name, 2) ?? string.Empty).Trim(),
                    ContainsSbo = g.Any(b => string.Equals(AuthHelper.GetValue(b.business_name, 2), "SBO", StringComparison.OrdinalIgnoreCase)),
                    Email = g.FirstOrDefault(b => !string.IsNullOrEmpty(b.email))?.email,
                    Branches = g.Select(b => new CustomerBranchViewModel
                    {
                        CustomerId = b.CustomerId,
                        BusinessType = (AuthHelper.GetValue(b.business_name, 1) ?? string.Empty).Trim(),
                        UserType = (AuthHelper.GetValue(b.business_name, 2) ?? string.Empty).Trim(),
                        BranchName = (AuthHelper.GetValue(b.business_name, 4) ?? string.Empty).Trim(),
                        City = b.city,
                        Status = b.registration_status,
                        IsMainBranch = !string.IsNullOrEmpty(b.email),
                        RdcId = b.RdcId,
                        Email = b.email,
                        StreetAddress = b.street_address,
                        ZipCode = b.zip_code,
                        PhoneNumber = b.phone_number,
                        IsActive = b.IsActive,
                        DisapprovedAt = b.DisapprovedAt
                    }).ToList()
                }).ToList();

            // Filter out SBO and match requested UserType
            var typeFiltered = clusters.Where(c => !c.ContainsSbo && string.Equals(c.UserType, requestedType, StringComparison.OrdinalIgnoreCase));

            tab = (tab ?? "pending").ToLowerInvariant();
            Func<CustomerBranchViewModel, bool> branchPredicate = tab switch
            {
                "pending" => b => string.Equals(b.Status, "PENDING", StringComparison.OrdinalIgnoreCase),
                "active" => b => string.Equals(b.Status, "APPROVED", StringComparison.OrdinalIgnoreCase) && b.IsActive,
                "suspended" => b => string.Equals(b.Status, "SUSPENDED", StringComparison.OrdinalIgnoreCase),
                "disapproved" => b => string.Equals(b.Status, "DISAPPROVED", StringComparison.OrdinalIgnoreCase),
                _ => b => true
            };

            return typeFiltered
                .Select(c => new CustomerClusterViewModel
                {
                    UniqueCode = c.UniqueCode,
                    BusinessName = c.BusinessName,
                    BusinessType = c.BusinessType,
                    UserType = c.UserType,
                    ContainsSbo = c.ContainsSbo,
                    Email = c.Email,
                    Branches = c.Branches.Where(branchPredicate).ToList()
                })
                .Where(c => c.Branches != null && c.Branches.Any())
                .ToList();
        }

        // Suspend selected branches (branchIds). If branchIds is empty/null, suspend whole cluster identified by UniqueCode.
        [HttpPost]
        public async Task<IActionResult> SuspendBranches([FromBody] ManageClusterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UniqueCode)) return BadRequest("Invalid request");
            // Materialize customers first because GetRegistrationCode() is a CLR method and cannot be translated by EF
            var allCustomers = await _context.Customers.ToListAsync();
            var branches = allCustomers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == request.UniqueCode).ToList();
            if (!branches.Any()) return NotFound();

            var branchIds = request.BranchIds ?? new List<int>();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                List<Customer> toSuspend;
                if (!branchIds.Any())
                {
                    toSuspend = branches;
                }
                else
                {
                    // Prevent suspending main branch alone
                    var mainBranchId = branches.FirstOrDefault(b => !string.IsNullOrEmpty(b.email))?.CustomerId;
                    if (mainBranchId.HasValue && branchIds.Contains(mainBranchId.Value) && branchIds.Count < branches.Count)
                        return BadRequest("Cannot suspend the main branch without suspending the entire cluster.");

                    toSuspend = branches.Where(b => branchIds.Contains(b.CustomerId)).ToList();
                }

                var userIds = toSuspend.Where(b => b.UserId.HasValue).Select(b => b.UserId!.Value).Distinct().ToList();
                foreach (var uid in userIds)
                {
                    var user = await _context.Users.FindAsync(uid);
                    if (user != null) _context.Users.Remove(user);
                }

                foreach (var b in toSuspend)
                {
                    b.IsActive = false;
                    b.registration_status = "SUSPENDED";
                    b.UserId = null;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // Permanently delete selected branches. If BranchIds empty, delete whole cluster only if allowed by business rules.
        [HttpPost]
        public async Task<IActionResult> DeleteBranches([FromBody] ManageClusterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UniqueCode)) return BadRequest("Invalid request");
            // Materialize customers first because GetRegistrationCode() is a CLR method and cannot be translated by EF
            var allCustomers = await _context.Customers.ToListAsync();
            var branches = allCustomers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == request.UniqueCode).ToList();
            if (!branches.Any()) return NotFound();

            var branchIds = request.BranchIds ?? new List<int>();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                List<Customer> toDelete;
                if (!branchIds.Any())
                {
                    // If attempting whole-cluster delete, ensure all are disapproved
                    if (!branches.All(b => string.Equals(b.registration_status, "DISAPPROVED", StringComparison.OrdinalIgnoreCase)))
                        return BadRequest("Can only permanently delete a fully disapproved cluster.");
                    toDelete = branches;
                }
                else
                {
                    toDelete = branches.Where(b => branchIds.Contains(b.CustomerId) && (b.IsActive == false || string.Equals(b.registration_status, "DISAPPROVED", StringComparison.OrdinalIgnoreCase))).ToList();
                    if (!toDelete.Any()) return BadRequest("Selected branches cannot be deleted. Ensure they are suspended or disapproved first.");
                }

                foreach (var b in toDelete)
                {
                    if (b.UserId.HasValue)
                    {
                        var u = await _context.Users.FindAsync(b.UserId.Value);
                        if (u != null) _context.Users.Remove(u);
                    }
                    _context.Customers.Remove(b);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }




        [HttpPost]
        public async Task<IActionResult> ManageClusterState([FromBody] ManageClusterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UniqueCode) || string.IsNullOrEmpty(request.Action))
                return BadRequest("Invalid request");

            var uniqueCode = request.UniqueCode;
            var action = request.Action.ToUpperInvariant();
            var rdcAssignments = request.RdcAssignments ?? new Dictionary<int, int>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Materialize customers first because GetRegistrationCode() is a CLR method and cannot be translated by EF
                var allCustomers = await _context.Customers.ToListAsync();
                var branches = allCustomers.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == uniqueCode).ToList();

                if (action == "APPROVE")
                {
                    // Approve entire cluster
                    foreach (var b in branches)
                    {
                        if (rdcAssignments.TryGetValue(b.CustomerId, out int rdc)) b.RdcId = rdc;
                        b.registration_status = "APPROVED";
                        b.IsActive = true;
                    }
                }
                else if (action == "SUSPEND")
                {
                    // Cluster-level suspension: deactivate branches and remove associated user accounts
                    foreach (var b in branches)
                    {
                        b.IsActive = false;
                        b.registration_status = "SUSPENDED";
                        // Clear any RDC assignments
                        b.RdcId = null;
                    }

                    // Remove associated users for this cluster (main branch user)
                    var userIds = branches.Where(b => b.UserId.HasValue).Select(b => b.UserId!.Value).Distinct().ToList();
                    foreach (var uid in userIds)
                    {
                        var user = await _context.Users.FindAsync(uid);
                        if (user != null) _context.Users.Remove(user);
                    }
                    // Clear UserId references
                    foreach (var b in branches) b.UserId = null;
                }
                else if (action == "DELETE")
                {
                    // Permanent delete entire cluster - only allow if all branches are DISAPPROVED
                    if (branches.All(b => b.registration_status == "DISAPPROVED"))
                    {
                        foreach (var b in branches) _context.Customers.Remove(b);
                    }
                    else return BadRequest("Can only permanently delete a fully disapproved cluster.");
                }
                else if (action == "DISAPPROVE")
                {
                    // Mark every branch in the cluster as disapproved and deactivate
                    foreach (var b in branches)
                    {
                        b.registration_status = "DISAPPROVED";
                        b.IsActive = false;
                        b.DisapprovedAt = DateTime.Now;
                    }

                    // Remove associated users
                    var userIds = branches.Where(b => b.UserId.HasValue).Select(b => b.UserId!.Value).Distinct().ToList();
                    foreach (var uid in userIds)
                    {
                        var user = await _context.Users.FindAsync(uid);
                        if (user != null) _context.Users.Remove(user);
                    }
                    foreach (var b in branches) b.UserId = null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendClusterEmail([FromBody] ClusterEmailRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrEmpty(req.UniqueCode))
                    return BadRequest(new { errors = new[] { "Invalid request payload." } });
                // req is model-bound
                var all = await _context.Customers.ToListAsync();
                var clusterBranches = all.Where(c => (c.GetRegistrationCode() ?? "SBO_" + c.CustomerId) == req.UniqueCode).ToList();
                var businessName = ISDN.Helpers.AuthHelper.GetValue(clusterBranches.FirstOrDefault()?.business_name, 3) ?? "Customer";
                var mainBranch = clusterBranches.FirstOrDefault(b => !string.IsNullOrEmpty(b.email));
                var mainEmail = mainBranch?.email ?? req.To;

                // Build placeholders
                var branchNames = clusterBranches.Where(b => req.BranchIds == null || !req.BranchIds.Any() || req.BranchIds.Contains(b.CustomerId))
                    .Select(b => ISDN.Helpers.AuthHelper.GetValue(b.business_name, 4)).Where(n => !string.IsNullOrEmpty(n)).ToList();

                string template;
                // Build HTML templates
                var branchNamesHtml = string.Join("", branchNames.Select(n => $"<li>{System.Net.WebUtility.HtmlEncode(n)}</li>"));

                if (string.Equals(req.Type, "approve", StringComparison.OrdinalIgnoreCase))
                {
                    template = $"<p>Dear {System.Net.WebUtility.HtmlEncode(businessName)},</p>" +
                               "<p>Your registration has been <strong>approved</strong>. You can log in using the email <strong>{Email}</strong> and the password you set during registration.</p>" +
                               "<p>{Notes}</p>" +
                               "<p>Regards,<br/>{AdminName}</p>";
                }
                else if (string.Equals(req.Type, "suspend", StringComparison.OrdinalIgnoreCase))
                {
                    template = $"<p>Dear {System.Net.WebUtility.HtmlEncode(businessName)},</p>" +
                               "<p>The following branch(es) have been <strong>suspended</strong>:</p>" +
                               "<ul>" + branchNamesHtml + "</ul>" +
                               "<p>{Notes}</p>" +
                               "<p>Regards,<br/>{AdminName}</p>";
                }
                else
                {
                    template = $"<p>Dear {System.Net.WebUtility.HtmlEncode(businessName)},</p>" +
                               "<p>Your registration cluster (Code: <strong>{UniqueCode}</strong>) has been <strong>permanently deleted</strong>.</p>" +
                               "<p>{Notes}</p>" +
                               "<p>Regards,<br/>{AdminName}</p>";
                }

                var populated = template
                    .Replace("{Email}", System.Net.WebUtility.HtmlEncode(mainEmail ?? ""))
                    .Replace("{BranchNames}", string.Join(", ", branchNames.Select(System.Net.WebUtility.HtmlEncode)))
                    .Replace("{UniqueCode}", System.Net.WebUtility.HtmlEncode(req.UniqueCode ?? ""))
                    .Replace("{AdminName}", System.Net.WebUtility.HtmlEncode(req.From ?? "ISDN Head Office"))
                    .Replace("{Notes}", System.Net.WebUtility.HtmlEncode(req.Message ?? ""));

                var smtpHost = _config["EmailSettings:SmtpServer"] ?? _config["Smtp:Host"];
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? _config["Smtp:Port"] ?? "587");
                var smtpUser = _config["EmailSettings:Username"] ?? _config["Smtp:User"];
                var smtpPass = _config["EmailSettings:Password"] ?? _config["Smtp:Pass"];

                using var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort);
                if (!string.IsNullOrEmpty(smtpUser))
                {
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                    client.EnableSsl = true;
                }

                var mail = new System.Net.Mail.MailMessage(req.From ?? "no-reply@isdn.local", req.To ?? mainEmail, req.Subject ?? "Notification", populated);
                mail.IsBodyHtml = true;
                await client.SendMailAsync(mail);

                // If this is a suspension, apply branch-level suspension if branchIds provided
                if (string.Equals(req.Type, "suspend", StringComparison.OrdinalIgnoreCase) && req.BranchIds != null && req.BranchIds.Any())
                {
                    var mainBranchId = mainBranch?.CustomerId;
                    // If admin is trying to suspend the main branch alone, disallow
                    if (mainBranchId.HasValue && req.BranchIds.Contains(mainBranchId.Value) && req.BranchIds.Count < clusterBranches.Count)
                    {
                        return BadRequest(new { errors = new[] { "Cannot suspend the main branch without suspending the entire cluster." } });
                    }

                    var branchesToSuspend = await _context.Customers.Where(c => req.BranchIds.Contains(c.CustomerId)).ToListAsync();
                    foreach (var b in branchesToSuspend)
                    {
                        b.IsActive = false;
                        b.registration_status = "SUSPENDED";
                    }

                    if (mainBranchId.HasValue && req.BranchIds.Contains(mainBranchId.Value))
                    {
                        var userIds = clusterBranches.Where(b => b.UserId.HasValue).Select(b => b.UserId!.Value).Distinct().ToList();
                        foreach (var uid in userIds)
                        {
                            var user = await _context.Users.FindAsync(uid);
                            if (user != null) _context.Users.Remove(user);
                        }
                        foreach (var b in clusterBranches) b.UserId = null;
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        public class ClusterEmailRequest
        {
            public string? UniqueCode { get; set; }
            public string? Type { get; set; }
            public string? To { get; set; }
            public string? From { get; set; }
            public string? Subject { get; set; }
            public string? Message { get; set; }
            public List<int>? BranchIds { get; set; }
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
