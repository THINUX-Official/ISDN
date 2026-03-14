using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISDN.Constants;
using ISDN.Data;
using ISDN.Helpers;
using ISDN.Models;
using ISDN.ViewModels;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.HeadOffice + "," + UserRoles.Admin)]
    public class CustomerAdminController : Controller
    {
        private readonly IsdnDbContext _db;

        public CustomerAdminController(IsdnDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ManageClustersDetails()
        {
            var customers = await _db.Customers
                .Include(c => c.Rdc)
                .Where(c => c.business_name != null && c.business_name.Contains("|"))
                .ToListAsync();

            var vm = new CustomerManagementViewModel();

            foreach (var c in customers)
            {
                var dto = new CustomerClusterDto
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    FirstName = c.first_name,
                    LastName = c.last_name,
                    Email = c.email ?? "",
                    StreetAddress = c.street_address,
                    City = c.city,
                    ZipCode = c.zip_code,
                    RdcId = c.RdcId,
                    RdcName = c.Rdc?.RdcName ?? "Unassigned",
                    RegistrationCode = c.GetRegistrationCode() ?? ""
                };

                dto.BusinessType = AuthHelper.GetValue(c.business_name, 1);
                dto.UserType = AuthHelper.GetValue(c.business_name, 2);
                dto.BusinessName = AuthHelper.GetValue(c.business_name, 3);
                dto.BranchName = AuthHelper.GetValue(c.business_name, 4);

                if (dto.UserType?.ToUpper() == "PBOS")
                {
                    vm.PbósCustomers.Add(dto);
                }
                else if (dto.UserType?.ToUpper() == "PBOM")
                {
                    vm.PbómCustomers.Add(dto);
                }
            }

            var rdcs = await _db.Rdcs.Where(r => r.IsActive).ToListAsync();
            vm.AvailableRdcs = rdcs.Select(r => new SelectListItem
            {
                Value = r.RdcId.ToString(),
                Text = r.RdcName
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(int customerId, string businessType, string businessName, string branchName, string streetAddress, string city, string zipCode, int? rdcId)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found." });
            }

            var currentUserType = AuthHelper.GetValue(customer.business_name, 2);
            
            // Format: "|[Type]|[UserType]|[BusinessName]|[BranchName]"
            string newBusinessNameStr = $"|{businessType}|{currentUserType}|{businessName}|{branchName}";

            customer.business_name = newBusinessNameStr;
            customer.street_address = streetAddress;
            customer.city = city;
            customer.zip_code = zipCode;
            customer.RdcId = rdcId;

            try
            {
                _db.Customers.Update(customer);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Customer details updated successfully." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the customer. " + ex.Message });
            }
        }
    }
}