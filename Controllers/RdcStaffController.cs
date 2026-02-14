using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Data;
using Microsoft.EntityFrameworkCore;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.RdcStaff)]
    public class RdcStaffController : BaseRdcController
    {
        private readonly IProductRepository _productRepository;
        private readonly IRdcOrderRepository _rdcOrderRepository; // මම මෙය වෙනස් කළා
        private readonly IsdnDbContext _context;

        public RdcStaffController(IProductRepository productRepository, IRdcOrderRepository rdcOrderRepository, IsdnDbContext context)
        {
            _productRepository = productRepository;
            _rdcOrderRepository = rdcOrderRepository;
            _context = context;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            ViewBag.RdcId = GetUserRdcId();
            ViewBag.IsHeadOffice = IsHeadOfficeUser();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            var inventoryQuery = _context.Inventories
                .Include(i => i.Product)
                .AsQueryable();

            inventoryQuery = ApplyRdcFilter(inventoryQuery);

            var inventory = await inventoryQuery
                .OrderBy(i => i.Product.ProductName)
                .ToListAsync();

            ViewBag.RdcId = GetUserRdcId();
            return View(inventory);
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            int rdcId = GetUserRdcId() ?? 0;

            // ViewModel එක පාවිච්චි කරලා filtered orders ටික ගන්නවා
            var ordersWithReturns = await _rdcOrderRepository.GetOrdersByRdcAsync(rdcId);

            ViewBag.RdcId = rdcId;
            return View(ordersWithReturns);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPacked(int orderId)
        {
            int adminId = GetUserId();
            bool success = await _rdcOrderRepository.UpdateOrderStatusAsync(orderId, "Packed", adminId);

            if (success)
            {
                TempData["SuccessMessage"] = "Order marked as Packed successfully!";
            }
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(int returnId, string status, string adminComment)
        {
            // ලොග් වී සිටින RDC Staff member ගේ User ID එක මෙතනින් ගන්නවා
            int adminId = GetUserId();

            if (string.IsNullOrEmpty(adminComment))
            {
                TempData["Error"] = "Please provide a comment for your decision.";
                return RedirectToAction(nameof(Orders));
            }

            bool success = await _rdcOrderRepository.ProcessReturnAsync(returnId, status, adminComment, adminId);

            if (success)
            {
                TempData["SuccessMessage"] = $"Return request has been {status.ToLower()} successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to process the return request.";
            }

            return RedirectToAction(nameof(Orders));
        }
    }
}
