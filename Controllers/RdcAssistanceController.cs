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
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using MailKit.Net.Smtp;

namespace ISDN.Controllers
{
    [Authorize(Roles = UserRoles.RdcStaff + "," + UserRoles.Admin + "," + UserRoles.HeadOffice)]
    public class RdcAssistanceController : BaseRdcController
    {
        private readonly IsdnDbContext _db;
        private readonly IConfiguration _config;

        public RdcAssistanceController(IsdnDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRdcId = GetUserRdcId();
            if (!userRdcId.HasValue)
            {
                return RedirectToAction("Index", "Home"); // Should belong to an RDC
            }

            var vm = new RdcPackRequestViewModel();

            var pendingOrders = await _db.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .Where(o => o.RdcId == userRdcId.Value && o.Status == "PLACED")
                .ToListAsync();

            vm.PendingOrders = pendingOrders.Select(o => new OrderAssistanceDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerName = o.Customer?.first_name + " " + o.Customer?.last_name,
                Items = o.OrderItems.Select(oi => new OrderAssistanceItemDto
                {
                    ProductName = oi.Product?.ProductName ?? "Unknown Product",
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();

            var otherRdcs = await _db.Rdcs.Where(r => r.RdcId != userRdcId.Value && r.IsActive).ToListAsync();
            vm.TargetRdcs = otherRdcs.Select(r => new SelectListItem
            {
                Value = r.RdcId.ToString(),
                Text = $"{r.RdcName} - {r.ContactNumber}"
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(int orderId, int targetRdcId)
        {
            var userRdcId = GetUserRdcId();
            if (!userRdcId.HasValue)
            {
                return Json(new { success = false, message = "User is not assigned to an RDC." });
            }

            var order = await _db.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.RdcId == userRdcId.Value);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found or not eligible." });
            }

            var targetRdc = await _db.Rdcs.FindAsync(targetRdcId);
            if (targetRdc == null)
            {
                return Json(new { success = false, message = "Target RDC not found." });
            }

            var sourceRdc = await _db.Rdcs.FindAsync(userRdcId.Value);

            // Find an Admin at the target RDC to email
            var targetUsers = await _db.Users
                .Where(u => u.RdcId == targetRdcId && u.IsActive && u.Email != null)
                .ToListAsync();

            if (!targetUsers.Any())
            {
                return Json(new { success = false, message = "Target RDC has no active users to email." });
            }

            // Prepare email content
            var acceptLink = Url.Action("AcceptRequest", "RdcAssistance", new { orderId = order.OrderId, newRdcId = targetRdcId }, Request.Scheme);

            var itemsHtml = "<ul>";
            foreach (var item in order.OrderItems)
            {
                itemsHtml += $"<li>{item.Product?.ProductName} (Qty: {item.Quantity})</li>";
            }
            itemsHtml += "</ul>";

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; background-color: #f8f9fa; padding: 20px;'>
                    <div style='background-color: #ffffff; padding: 20px; border-radius: 5px; max-width: 600px; margin: 0 auto; border: 1px solid #e9ecef;'>
                        <h2 style='color: #007bff; border-bottom: 2px solid #007bff; padding-bottom: 10px;'>Cross-RDC Packing Request</h2>
                        <p>Hello {targetRdc.RdcName} Team,</p>
                        <p><strong>{sourceRdc?.RdcName}</strong> is requesting your assistance to pack an order due to local stock shortages.</p>
                        <p><strong>Order Number:</strong> {order.OrderNumber}</p>
                        <p><strong>Items requires:</strong></p>
                        {itemsHtml}
                        <p>If you have the necessary stock and are able to fulfill this order, please click the button below to accept the request.</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{acceptLink}' style='background-color: #28a745; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>Accept Packing Request</a>
                        </div>
                        <hr style='border-top: 1px solid #eee; margin: 20px 0;'/>
                        <p style='color: #6c757d; font-size: 12px;'>Automated Request from ISDN Cross-RDC System</p>
                    </div>
                </div>";

            try
            {
                var smtpHost = _config["EmailSettings:SmtpServer"] ?? _config["Smtp:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.TryParse(_config["EmailSettings:SmtpPort"] ?? _config["Smtp:Port"], out var p) ? p : 587;
                var smtpUser = _config["EmailSettings:Username"] ?? _config["Smtp:User"];
                var smtpPass = _config["EmailSettings:Password"] ?? _config["Smtp:Pass"];
                var fromName = _config["EmailSettings:SenderName"] ?? _config["Smtp:FromName"] ?? "ISDN RDC Support";
                var fromEmail = _config["EmailSettings:SenderEmail"] ?? _config["Smtp:FromEmail"] ?? smtpUser ?? "no-reply@isdn.local";

                var message = new MimeMessage();
                var effectiveFrom = !string.IsNullOrEmpty(smtpUser) ? smtpUser : fromEmail;
                message.From.Add(new MailboxAddress(fromName, effectiveFrom));
                
                // Add all target users
                foreach (var user in targetUsers)
                {
                    message.To.Add(MailboxAddress.Parse(user.Email));
                }
                
                message.Subject = $"Assistance Request: Pack Order {order.OrderNumber}";

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                // Depending on config we could use Connect options but sticking to defaults as in Payments
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
                {
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Json(new { success = true, message = "Email request sent successfully." });
            }
            catch (Exception ex)
            {
                // In production, log error
                return Json(new { success = false, message = "Failed to send email. " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AcceptRequest(int orderId, int newRdcId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            var targetRdc = await _db.Rdcs.FindAsync(newRdcId);
            if (targetRdc == null)
            {
                TempData["ErrorMessage"] = "Target RDC not found.";
                return RedirectToAction("Index", "Home");
            }

            // Only allow transferring if it is currently PLACED
            if (order.Status != "PLACED")
            {
                TempData["ErrorMessage"] = "Order is no longer available for transfer.";
                return RedirectToAction("Index", "Home");
            }

            order.RdcId = newRdcId;

            var log = new OrderStatusLog
            {
                OrderId = order.OrderId,
                Status = $"Fulfillment transferred to RDC {targetRdc.RdcName}",
                CreatedAt = DateTime.UtcNow,
                UpdatedById = GetUserId() == 0 ? 1 : GetUserId() // Fallback if no user is authenticated during link click
            };
            _db.OrderStatusLogs.Add(log);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order {order.OrderNumber} successfully accepted by your RDC.";
            return RedirectToAction("Index", "Orders"); // Redirect to orders list
        }
    }
}