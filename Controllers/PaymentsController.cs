using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ISDN.Data;
using ISDN.Services;
using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ISDN.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ISDN.Data.IsdnDbContext _db;
        private readonly ISDN.Services.PayPalService _paypal;

        public PaymentsController(IConfiguration config, ISDN.Data.IsdnDbContext db, ISDN.Services.PayPalService paypal)
        {
            _config = config;
            _db = db;
            _paypal = paypal;
        }

        // More detailed Order invoice document including items and totals
        private class OrderInvoiceDocument : IDocument
        {
            private readonly ISDN.Models.Order _order;
            private readonly System.Collections.Generic.List<ISDN.Models.OrderItem> _items;
            private readonly string? _recipient;

            public OrderInvoiceDocument(ISDN.Models.Order order, System.Collections.Generic.List<ISDN.Models.OrderItem> items, string? recipient = null)
            {
                _order = order;
                _items = items;
                _recipient = recipient;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Row(row =>
                    {
                        row.RelativeColumn().Stack(stack =>
                        {
                            stack.Item().Text("ISDN").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            stack.Item().Text("Invoice").FontSize(12).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantColumn(160).AlignRight().Stack(stack =>
                        {
                            stack.Item().Text($"Invoice: {_order.OrderNumber ?? ("#" + _order.OrderId)}").FontSize(10).FontColor(Colors.Grey.Darken2);
                            stack.Item().Text($"Date: {_order.OrderDate:dd MMM yyyy}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    page.Content().PaddingVertical(8).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Product");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Qty");
                                header.Cell().Element(CellStyle).AlignRight().Text("Subtotal");
                            });

                            foreach (var it in _items)
                            {
                                table.Cell().Element(CellStyle).Text(it.Product?.ProductName ?? "Item");
                                table.Cell().Element(CellStyle).AlignCenter().Text(it.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"Rs. {it.Subtotal:N2}");
                            }

                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(2).Element(CellStyle).AlignRight().Text("Total");
                                footer.Cell().Element(CellStyle).AlignRight().Text($"Rs. {_items.Sum(x => x.Subtotal):N2}");
                            });
                        });
                    });

                    page.Footer().AlignRight().Text($"Thank you for your purchase - ISDN").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            }

            static IContainer CellStyle(IContainer container)
            {
                return container.Padding(6).Border(1).BorderColor(Colors.Grey.Lighten3);
            }
        }

        public class SendInvoiceRequest
        {
            public string? To { get; set; }
            public string? Subject { get; set; }
            public string? Message { get; set; }
            public string? Html { get; set; }
            // Optional: if provided, server will load the order and include order details in the invoice
            public int? OrderId { get; set; }
        }

        private async Task<(bool Success, string? Error)> SendMailAsync(string to, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null)
        {
            try
            {
                var smtpHost = _config["EmailSettings:SmtpServer"] ?? _config["Smtp:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.TryParse(_config["EmailSettings:SmtpPort"] ?? _config["Smtp:Port"], out var p) ? p : 587;
                var smtpUser = _config["EmailSettings:Username"] ?? _config["Smtp:User"];
                var smtpPass = _config["EmailSettings:Password"] ?? _config["Smtp:Pass"];
                var fromName = _config["EmailSettings:SenderName"] ?? _config["Smtp:FromName"] ?? "ISDN";
                var fromEmail = _config["EmailSettings:SenderEmail"] ?? _config["Smtp:FromEmail"] ?? smtpUser ?? "no-reply@isdn.local";

                var message = new MimeMessage();
                var effectiveFrom = !string.IsNullOrEmpty(smtpUser) ? smtpUser : fromEmail;
                message.From.Add(new MailboxAddress(fromName, effectiveFrom));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                if (attachment != null && attachmentName != null)
                {
                    builder.Attachments.Add(attachmentName, attachment);
                }

                message.Body = builder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                SecureSocketOptions socketOptions = SecureSocketOptions.Auto;
                if (smtpPort == 587) socketOptions = SecureSocketOptions.StartTls;
                if (smtpPort == 465) socketOptions = SecureSocketOptions.SslOnConnect;

                await client.ConnectAsync(smtpHost, smtpPort, socketOptions);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrEmpty(smtpUser)) await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return (true, null);
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Quick invoice PDF generator using QuestPDF for a polished layout
        private class InvoiceDocument : IDocument
        {
            private readonly string _title;
            private readonly string? _message;
            private readonly string? _htmlContent;
            private readonly DateTime _generated;
            private readonly string? _recipient;

            public InvoiceDocument(string title, string? message, string? htmlContent, string? recipient = null)
            {
                _title = title ?? "ISDN Invoice";
                _message = message;
                _htmlContent = htmlContent;
                _generated = DateTime.Now;
                _recipient = recipient;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Height(60).Row(row =>
                    {
                        row.RelativeColumn().Stack(stack =>
                        {
                            stack.Item().Text("ISDN").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            stack.Item().Text(_title).FontSize(12).FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantColumn(160).AlignRight().Stack(stack =>
                        {
                            stack.Item().Text($"Date: {_generated:dd MMM yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                            if (!string.IsNullOrEmpty(_recipient)) stack.Item().Text(_recipient).FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        if (!string.IsNullOrEmpty(_message))
                        {
                            col.Item().Text(_message).FontSize(11);
                            col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        }

                        if (!string.IsNullOrEmpty(_htmlContent))
                        {
                            // Strip HTML tags for a simple plain-text rendering inside the PDF
                            var plain = Regex.Replace(_htmlContent, "<.*?>", string.Empty);
                            // Limit overly long content
                            if (plain.Length > 10000) plain = plain.Substring(0, 10000) + "...";
                            col.Item().Text(plain).FontSize(10).FontColor(Colors.Grey.Darken2);
                        }
                        else
                        {
                            col.Item().Text("No invoice details provided.").FontColor(Colors.Grey.Lighten2);
                        }
                    });

                    page.Footer().Height(40).AlignCenter().Text(text =>
                    {
                        text.Span("Thank you for your purchase. ").SemiBold().FontSize(10).FontColor(Colors.Grey.Medium);
                        text.Span("ISDN").FontColor(Colors.Blue.Darken2).FontSize(10);
                    });
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvoice([FromBody] SendInvoiceRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.To)) return Json(new { success = false, message = "Recipient required." });

            var smtpHost = _config["EmailSettings:SmtpServer"] ?? _config["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.TryParse(_config["EmailSettings:SmtpPort"] ?? _config["Smtp:Port"], out var p) ? p : 587;
            var smtpUser = _config["EmailSettings:Username"] ?? _config["Smtp:User"];
            var smtpPass = _config["EmailSettings:Password"] ?? _config["Smtp:Pass"];
            var fromName = _config["EmailSettings:SenderName"] ?? _config["Smtp:FromName"] ?? "ISDN";
            var fromEmail = _config["EmailSettings:SenderEmail"] ?? _config["Smtp:FromEmail"] ?? smtpUser ?? "no-reply@isdn.local";

            try
            {
                var htmlBody = "<div>" + (req.Message ?? string.Empty) + "</div>" + (req.Html ?? string.Empty);

                // Generate a polished PDF invoice using QuestPDF and attach it
                byte[]? pdfBytes = null;
                try
                {
                    var doc = new InvoiceDocument(req.Subject ?? "ISDN Invoice", req.Message, req.Html, req.To);
                    using var ms = new System.IO.MemoryStream();
                    doc.GeneratePdf(ms);
                    pdfBytes = ms.ToArray();
                }
                catch (System.Exception ex)
                {
                    // If PDF generation fails, log and continue without attachment
                    // Logging service not available here; swallow the exception
                    pdfBytes = null;
                }

                var (sent, err) = await SendMailAsync(req.To, req.Subject ?? "ISDN Invoice", htmlBody, pdfBytes, pdfBytes != null ? "invoice.pdf" : null);
                if (!sent) return Json(new { success = false, message = err ?? "Failed to send email." });

                return Json(new { success = true, message = "Invoice sent successfully." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPayPalOrder([FromBody] string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) return BadRequest("orderId required");

            var (ok, doc) = await _paypal.VerifyOrderAsync(orderId);
            if (!ok || doc == null) return BadRequest("Failed to verify order with PayPal");

            // Expect status COMPLETED
            if (doc.RootElement.TryGetProperty("status", out var statusEl) && statusEl.GetString() == "COMPLETED")
            {
                // Extract purchase units and amount
                var amount = "0";
                var captureId = (string?)null;
                if (doc.RootElement.TryGetProperty("purchase_units", out var pus) && pus.GetArrayLength() > 0)
                {
                    var pu = pus[0];
                    if (pu.TryGetProperty("payments", out var payments) && payments.TryGetProperty("captures", out var captures) && captures.GetArrayLength() > 0)
                    {
                        var cap = captures[0];
                        if (cap.TryGetProperty("id", out var id)) captureId = id.GetString();
                        if (cap.TryGetProperty("amount", out var a) && a.TryGetProperty("value", out var v)) amount = v.GetString() ?? "0";
                    }
                }

                // TODO: map capture or order to local order id. For now only record as a payment without order link
                var payment = new ISDN.Models.Payment
                {
                    OrderId = 0,
                    RdcId = null,
                    Amount = decimal.TryParse(amount, out var d) ? d : 0m,
                    PaymentMethod = "PayPal",
                    PaymentStatus = "Completed",
                    TransactionId = orderId,
                    PaymentDate = DateTime.Now,
                    CreatedAt = DateTime.UtcNow
                };
                try
                {
                    await _db.Payments.AddAsync(payment);
                    await _db.SaveChangesAsync();
                }
                catch (System.Exception ex)
                {
                    return StatusCode(500, "Failed to record payment: " + ex.Message);
                }

                return Ok(new { success = true, transaction = orderId, captureId });
            }

            return BadRequest(new { success = false, status = doc.RootElement.GetProperty("status").GetString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefundPayPal([FromBody] JsonElement payload)
        {
            // Expect payload with captureId, amount, currency and recipientEmail, orderId(optional)
            if (!payload.TryGetProperty("captureId", out var capEl)) return BadRequest("captureId required");
            var captureId = capEl.GetString();
            var amount = payload.TryGetProperty("amount", out var a) ? a.GetString() ?? "0" : "0";
            var currency = payload.TryGetProperty("currency", out var c) ? c.GetString() ?? "USD" : "USD";
            var email = payload.TryGetProperty("email", out var e) ? e.GetString() ?? string.Empty : string.Empty;
            var orderId = payload.TryGetProperty("orderId", out var oi) ? (int?)(oi.GetInt32()) : null;

            if (string.IsNullOrEmpty(captureId)) return BadRequest("captureId required");

            var (ok, refundDoc) = await _paypal.RefundCaptureAsync(captureId, amount, currency);
            if (!ok) return StatusCode(500, new { success = false, message = "PayPal refund failed", details = refundDoc?.RootElement.ToString() });

            // Get refund id
            var refundId = refundDoc?.RootElement.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();

            // Record refund in payments table
            var payment = new ISDN.Models.Payment
            {
                OrderId = orderId ?? 0,
                RdcId = null,
                Amount = decimal.TryParse(amount, out var dec) ? dec : 0m,
                PaymentMethod = "PayPal Refund",
                PaymentStatus = "Refunded",
                TransactionId = refundId,
                PaymentDate = DateTime.Now,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _db.Payments.AddAsync(payment);
                await _db.SaveChangesAsync();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "Failed to record refund: " + ex.Message);
            }

            // Generate simple PDF receipt using PdfSharpCore
            byte[]? pdfBytes = null;
            try
            {
                using var ms = new System.IO.MemoryStream();
                var doc = new PdfSharpCore.Pdf.PdfDocument();
                var page = doc.AddPage();
                var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                var font = new PdfSharpCore.Drawing.XFont("Verdana", 14, PdfSharpCore.Drawing.XFontStyle.Bold);
                gfx.DrawString("Refund Receipt", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(0, 20, page.Width, 40), PdfSharpCore.Drawing.XStringFormats.TopCenter);
                var font2 = new PdfSharpCore.Drawing.XFont("Verdana", 10);
                gfx.DrawString($"Refund ID: {refundId}", font2, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(40, 80, page.Width - 80, 20), PdfSharpCore.Drawing.XStringFormats.TopLeft);
                gfx.DrawString($"Amount: {amount} {currency}", font2, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(40, 100, page.Width - 80, 20), PdfSharpCore.Drawing.XStringFormats.TopLeft);
                gfx.DrawString($"Date: {DateTime.Now}", font2, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(40, 120, page.Width - 80, 20), PdfSharpCore.Drawing.XStringFormats.TopLeft);
                if (!string.IsNullOrEmpty(email)) gfx.DrawString($"Recipient: {email}", font2, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(40, 140, page.Width - 80, 20), PdfSharpCore.Drawing.XStringFormats.TopLeft);
                doc.Save(ms);
                ms.Position = 0;
                pdfBytes = ms.ToArray();
            }
            catch
            {
                // ignore pdf errors
            }

            // Send confirmation email with attachment
            if (!string.IsNullOrEmpty(email))
            {
                var html = $"<p>Your refund of {amount} {currency} has been processed. Refund ID: {refundId}</p>";
                var (sent, err) = await SendMailAsync(email, "Refund Processed", html, pdfBytes, "refund_receipt.pdf");
                if (!sent)
                {
                    // Non-fatal: return success but indicate email failed
                    return Ok(new { success = true, refundId, emailSent = false, emailError = err });
                }
            }

            return Ok(new { success = true, refundId });
        }
    }
}
