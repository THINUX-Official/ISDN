using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISDN.Constants;
using ISDN.Data;
using ISDN.Models;
using ISDN.ViewModels;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.RdcStaff + "," + UserRoles.HeadOffice + "," + UserRoles.Admin)]
    public class OrderLifecycleController : BaseRdcController
    {
        private readonly IsdnDbContext _db;

        public OrderLifecycleController(IsdnDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? orderNumber)
        {
            var vm = new OrderLifecycleViewModel();

            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return View(vm);
            }

            orderNumber = orderNumber.Trim();

            var orderQuery = _db.Orders.AsQueryable();
            // orderQuery = ApplyRdcFilter(orderQuery); // allow tracking any order lifecycle to prevent "Order not found" if it belongs to another RDC or no RDC yet

            var order = await orderQuery
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Include(o => o.Deliveries)
                .Include(o => o.OrderStatusLogs)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) 
            {
                ModelState.AddModelError("", "Order not found.");
                return View(vm);
            }

            vm.OrderId = order.OrderId;
            vm.OrderNumber = order.OrderNumber;
            vm.OrderDate = order.OrderDate;
            vm.TotalAmount = order.TotalAmount;

            // Build virtual milestones
            // 1. Order Received
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Order Received",
                Status = "Completed",
                Timestamp = order.OrderDate,
                Icon = "fa-receipt",
                Description = "Order placed by customer"
            });

            // 2. Payment Verified (if any completed payment exists)
            var payment = order.Payments?.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Payment Verified",
                Status = payment != null && payment.PaymentStatus == "Completed" ? "Completed" : (order.Status.Contains("PAID", StringComparison.OrdinalIgnoreCase) || order.Status.Contains("COMPLETED", StringComparison.OrdinalIgnoreCase) ? "Completed" : "Future"),
                Timestamp = payment?.PaymentDate,
                Icon = "fa-credit-card",
                Description = payment != null ? $"{payment.PaymentMethod} - {payment.TransactionId ?? ""}" : "No payment recorded"
            });

            // 3. Inventory Allocation (check Inventory for each item in same RDC)
            var items = order.OrderItems?.ToList() ?? new System.Collections.Generic.List<OrderItem>();
            bool allAllocated = true;
            var allocationDetails = new System.Text.StringBuilder();
            foreach (var it in items)
            {
                var inv = await _db.Inventories.Where(i => i.ProductId == it.ProductId && i.RdcId == order.RdcId).FirstOrDefaultAsync();
                if (inv == null || inv.QuantityAvailable - inv.QuantityReserved < it.Quantity) allAllocated = false;
                allocationDetails.AppendLine($"{it.Product?.ProductName ?? "Item"} x{it.Quantity} - Available: {(inv?.QuantityAvailable ?? 0)}\n");
            }

            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Inventory Allocation",
                Status = allAllocated ? "Completed" : "InProgress",
                Timestamp = allAllocated ? DateTime.Now : (DateTime?)null,
                Icon = allAllocated ? "fa-box" : "fa-box-open",
                Description = allAllocated ? "All items reserved in RDC" : "Allocation pending or partial",
                DetailsHtml = allocationDetails.ToString()
            });

            // 4. Processing / Packing
            // infer from OrderStatusLogs or a heuristic: if status contains 'Processing' or 'Packed'
            var lastStatus = order.OrderStatusLogs?.OrderByDescending(s => s.CreatedAt).FirstOrDefault()?.Status;
            var processingStatus = lastStatus != null && (lastStatus.Contains("PROCESS", StringComparison.OrdinalIgnoreCase) || lastStatus.Contains("PACK", StringComparison.OrdinalIgnoreCase));
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Processing / Packing",
                Status = processingStatus ? "InProgress" : (order.Status.Contains("PLACED", StringComparison.OrdinalIgnoreCase) || order.Status.Contains("PROCESS", StringComparison.OrdinalIgnoreCase) ? "InProgress" : "Future"),
                Timestamp = order.OrderStatusLogs?.OrderByDescending(s => s.CreatedAt).FirstOrDefault(s => s.Status != null && (s.Status.Contains("PROCESS", StringComparison.OrdinalIgnoreCase) || s.Status.Contains("PACK", StringComparison.OrdinalIgnoreCase)))?.CreatedAt,
                Icon = "fa-boxes",
                Description = "Warehouse staff picking and packing items"
            });

            // 5. Ready for Dispatch
            var readyForDispatch = order.Status.Contains("READY", StringComparison.OrdinalIgnoreCase) || order.OrderStatusLogs?.Any(s => s.Status != null && s.Status.Contains("READY", StringComparison.OrdinalIgnoreCase)) == true;
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Ready for Dispatch",
                Status = readyForDispatch ? "Completed" : "Future",
                Timestamp = readyForDispatch ? order.OrderStatusLogs?.OrderByDescending(s => s.CreatedAt).FirstOrDefault(s => s.Status != null && s.Status.Contains("READY", StringComparison.OrdinalIgnoreCase))?.CreatedAt : null,
                Icon = "fa-truck-loading",
                Description = "Package ready at RDC for handover to logistics"
            });

            // 6. In Transit (based on Deliveries with non-null tracking and scheduled)
            var delivery = order.Deliveries?.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
            var inTransit = delivery != null && (delivery.Status?.Contains("In Transit", StringComparison.OrdinalIgnoreCase) == true || (!string.IsNullOrEmpty(delivery.TrackingNumber) && delivery.Status != "Delivered"));
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "In Transit",
                Status = inTransit ? "InProgress" : "Future",
                Timestamp = delivery?.ScheduledDate,
                Icon = "fa-truck",
                Description = delivery != null ? $"Carrier status: {delivery.Status} - Tracking: {delivery.TrackingNumber}" : "Not handed to carrier yet",
                DetailsHtml = delivery != null ? System.Net.WebUtility.HtmlEncode(delivery.Notes ?? string.Empty) : null
            });

            // 7. Out for Delivery
            var outForDelivery = delivery != null && delivery.Status != null && delivery.Status.Contains("Out for Delivery", StringComparison.OrdinalIgnoreCase);
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Out for Delivery",
                Status = outForDelivery ? "InProgress" : "Future",
                Timestamp = delivery?.ScheduledDate,
                Icon = "fa-biking",
                Description = outForDelivery ? "Assigned to driver and en route" : "Not out for delivery yet"
            });

            // 8. Delivered
            var delivered = delivery != null && (delivery.Status != null && delivery.Status.Contains("Delivered", StringComparison.OrdinalIgnoreCase) || delivery.DeliveryDate.HasValue);
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Delivered",
                Status = delivered ? "Completed" : "Future",
                Timestamp = delivery?.DeliveryDate,
                Icon = "fa-check-double",
                Description = delivered ? "Delivery confirmed" : "Awaiting delivery"
            });

            // 9. Post-Delivery Inspection Window (assume 7 days after delivery)
            DateTime? deliveredAt = delivery?.DeliveryDate;
            DateTime? inspectionWindowEnd = deliveredAt?.AddDays(7);
            var inspectionStatus = deliveredAt.HasValue ? (DateTime.Now <= inspectionWindowEnd ? "InProgress" : "Completed") : "Future";
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Post-Delivery Inspection Window",
                Status = inspectionStatus,
                Timestamp = deliveredAt,
                Icon = "fa-search",
                Description = deliveredAt.HasValue ? $"Inspection window ends {inspectionWindowEnd:dd MMM yyyy}" : "Not started"
            });

            // 10. Return Window Closed / Order Finalized (assume 14 days post-delivery)
            var finalizationDate = deliveredAt?.AddDays(14);
            var finalized = finalizationDate.HasValue && DateTime.Now > finalizationDate.Value;
            vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
            {
                Name = "Return Window Closed / Order Finalized",
                Status = finalized ? "Completed" : "Future",
                Timestamp = finalizationDate,
                Icon = "fa-flag-checkered",
                Description = finalized ? "Order lifecycle finalized" : "Still within return window"
            });

            // Conditional branching for returns
            var returns = await _db.OrderReturns.Where(r => r.OrderId == order.OrderId).ToListAsync();
            if (returns.Any())
            {
                vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
                {
                    Name = "Return Requested",
                    Status = "Issue",
                    Timestamp = returns.Min(r => r.CreatedAt),
                    Icon = "fa-undo",
                    Description = "Customer requested a return",
                    DetailsHtml = string.Join("<br/>", returns.Select(r => System.Net.WebUtility.HtmlEncode(r.AdminComment ?? "")))
                });

                vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
                {
                    Name = "Evidence Review",
                    Status = returns.Any(r => r.AdminStatus != null && r.AdminStatus.Contains("PENDING", StringComparison.OrdinalIgnoreCase)) ? "InProgress" : "Completed",
                    Timestamp = returns.OrderBy(r => r.CreatedAt).FirstOrDefault()?.CreatedAt,
                    Icon = "fa-eye",
                    Description = "Admin reviewing return evidence",
                    DetailsHtml = string.Join("<br/>", returns.Select(r => System.Net.WebUtility.HtmlEncode(r.AdminComment ?? "")))
                });

                vm.Milestones.Add(new OrderLifecycleViewModel.Milestone
                {
                    Name = "Refund Processed",
                    Status = returns.Any(r => r.RefundStatus != null && r.RefundStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase)) ? "Completed" : "Future",
                    Timestamp = returns.Where(r => r.RefundStatus != null && r.RefundStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase)).Select(r => (DateTime?)r.CreatedAt).FirstOrDefault(),
                    Icon = "fa-money-bill-wave",
                    Description = "Refund processed for returned items"
                });
            }

            return View(vm);
        }
    }
}
