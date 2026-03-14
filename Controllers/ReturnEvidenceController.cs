using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ISDN.Data;
using ISDN.ViewModels;
using MimeKit;
using MailKit.Security;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ISDN.Models;

namespace ISDN.Controllers
{
    public class ReturnEvidenceController : Controller
    {
        private readonly IsdnDbContext _db;
        private readonly IConfiguration _config;

        public ReturnEvidenceController(IsdnDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        private class EvidenceDoc : IDocument
        {
            private readonly OrderReturn _return;
            private readonly System.Collections.Generic.List<OrderItem> _items;

            public EvidenceDoc(OrderReturn ret, System.Collections.Generic.List<OrderItem> items)
            {
                _return = ret;
                _items = items;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text("ISDN - Return Evidence Instructions").FontSize(18).SemiBold();

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().PaddingTop(0).Text($"Order: {_return.Order?.OrderNumber ?? _return.OrderId.ToString()}").SemiBold();
                        col.Item().PaddingTop(2).Text($"Return ID: {_return.ReturnId}").SemiBold();
                        col.Item().PaddingTop(6).Text("Items to provide photos for:").Bold();

                        col.Item().PaddingTop(4).Column(list =>
                        {
                            foreach (var it in _items)
                            {
                                var name = it.Product?.ProductName ?? "Item";
                                list.Item().PaddingVertical(2).Row(r =>
                                {
                                    r.RelativeColumn().Text(name).FontSize(11);
                                    r.ConstantColumn(80).AlignRight().Text($"x{it.Quantity}").FontSize(11);
                                });
                            }
                        });

                        col.Item().PaddingTop(10).Text("Tips for good photos:").Bold();

                        col.Item().PaddingTop(4).Column(bcol =>
                        {
                            bcol.Item().Text("• Use good lighting and focus on damaged areas.");
                            bcol.Item().Text("• Include a photo of the whole product and the packaging.");
                            bcol.Item().Text("• Attach the images in your reply to this email.");
                        });

                        col.Item().PaddingTop(12).Text("Thank you,\nISDN Returns Team");
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Generated: ").FontSize(9);
                        txt.Span(System.DateTime.Now.ToString("g")).FontSize(9);
                    });
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Find returns where admin_status or refund_status indicates pending
            var returns = await _db.OrderReturns
                .Include(r => r.Order).ThenInclude(o => o.Customer)
                .Where(r => r.AdminStatus == "PENDING" || r.RefundStatus == "PENDING")
                .ToListAsync();

            var vm = returns.Select(r => new PendingReturnViewModel
            {
                ReturnId = r.ReturnId,
                OrderId = r.OrderId,
                OrderNumber = r.Order?.OrderNumber,
                AdminComment = r.AdminComment,
                Status = "Pending",
                CustomerName = r.Order?.Customer != null ? (r.Order.Customer.first_name + " " + r.Order.Customer.last_name).Trim() : "",
                CustomerEmail = r.Order?.Customer?.email,
                Items = _db.OrderItems.Where(oi => oi.OrderId == r.OrderId).Include(oi => oi.Product).Select(oi => new PendingReturnItemViewModel
                {
                    ProductName = oi.Product != null ? oi.Product.ProductName : "",
                    Quantity = oi.Quantity,
                    Price = oi.Subtotal
                }).ToList()
            }).ToList();

            return View("~/Views/ReturnProcessing/PendingReturnsValidityCheck.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEvidenceRequest(int returnId)
        {
            var ret = await _db.OrderReturns.Include(r => r.Order).ThenInclude(o => o.Customer).FirstOrDefaultAsync(r => r.ReturnId == returnId);
            if (ret == null) return NotFound();

            var items = await _db.OrderItems.Where(oi => oi.OrderId == ret.OrderId).Include(oi => oi.Product).ToListAsync();
            var productList = string.Join(", ", items.Select(i => $"{i.Product?.ProductName ?? "Item"} (x{i.Quantity})"));

            var customer = ret.Order?.Customer;
            var to = customer?.email;
            if (string.IsNullOrEmpty(to)) return BadRequest("Customer email not available.");

            var subject = $"Action Required: Evidence Images";

            // Build HTML email body
            var productHtmlList = string.Join("", items.Select(i => $"<li>{System.Net.WebUtility.HtmlEncode(i.Product?.ProductName ?? "Item")} (x{i.Quantity}) - Rs. {i.Subtotal:F2}</li>"));
            var html = $"<div style='font-family:Segoe UI, Arial, sans-serif; color:#222;'>\n                <h2 style='color:#1a237e;'>ISDN - Return Evidence Request</h2>\n                <p>Hello {System.Net.WebUtility.HtmlEncode(customer?.first_name ?? string.Empty)},</p>\n                <p>To proceed with your return, please provide clear photos showing the condition of the following items:</p>\n                <ul>{productHtmlList}</ul>\n                <p>Please reply to this email with the images attached. Make sure the photos show the damaged areas and the product packaging where applicable.</p>\n                <p style='color:#555;'>If you have any questions, reply to this email and our returns team will assist you.</p>\n                <p>Thank you,<br/>ISDN Returns Team</p>\n            </div>";

            var plain = $"Hello {customer?.first_name},\n\nPlease provide photos for: {productList}.\n\nReply to this email with images attached.\n\nThank you, ISDN Returns Team";

            // Create PDF instructions attachment using QuestPDF for a polished layout
            byte[]? pdfBytes = null;
            try
            {
                using var ms = new System.IO.MemoryStream();
                var document = new EvidenceDoc(ret, items);
                document.GeneratePdf(ms);
                ms.Position = 0;
                pdfBytes = ms.ToArray();
            }
            catch
            {
                // ignore pdf generation errors
            }

            // Build message with HTML and PDF attachment
            var message = new MimeMessage();
            var fromEmail = _config["EmailSettings:SenderEmail"] ?? _config["Smtp:FromEmail"] ?? _config["Smtp:User"] ?? "no-reply@isdn.local";
            var fromName = _config["EmailSettings:SenderName"] ?? _config["Smtp:FromName"] ?? "ISDN Returns";
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = html, TextBody = plain };
            if (pdfBytes != null)
            {
                builder.Attachments.Add("return_instructions.pdf", pdfBytes);
            }
            message.Body = builder.ToMessageBody();

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                var host = _config["EmailSettings:SmtpServer"] ?? _config["Smtp:Host"] ?? "smtp.gmail.com";
                var port = int.TryParse(_config["EmailSettings:SmtpPort"] ?? _config["Smtp:Port"], out var p) ? p : 587;
                var user = _config["EmailSettings:Username"] ?? _config["Smtp:User"];
                var pass = _config["EmailSettings:Password"] ?? _config["Smtp:Pass"];

                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrEmpty(user)) await client.AuthenticateAsync(user, pass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                // Update return to mark evidence requested and note attachment
                ret.AdminComment = (ret.AdminComment ?? "") + $"\nEvidence requested by admin at {System.DateTime.Now} (email sent)";
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Evidence request sent." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
