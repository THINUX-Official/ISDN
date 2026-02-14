using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISDN.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ISDN.Controllers
{
    public class FinanceController : Controller
    {
        private readonly AppDbContext _context;

        public FinanceController(AppDbContext context)
        {
            _context = context;
        }

        // 
        //  ADMIN REVENUE SECTION 
        // 
        public IActionResult AdminRevenue()
        {
            try
            {
                
                int userRdcId = 4;

                var payments = _context.payments.Where(p => p.rdc_id == userRdcId).AsNoTracking().ToList();
                var returns = _context.order_returns.Where(r => _context.orders.Any(o => o.order_id == r.order_id && o.rdc_id == userRdcId)).AsNoTracking().ToList();

                decimal revenue = payments.Sum(p => p.amount) - returns.Sum(r => (decimal?)r.refund_amount ?? 0m);

                ViewBag.FinalRevenue = revenue;
                ViewBag.TotalOrders = payments.Count;
                ViewBag.TotalReturns = returns.Sum(r => (decimal?)r.refund_amount ?? 0m);
                ViewBag.UserRdcName = "North RDC";
                ViewBag.RdcProfit = revenue;

                // Graph 
               
                ViewBag.RevenueData = payments
                    .GroupBy(p => p.payment_date?.Date ?? DateTime.Today)
                    .Select(g => new {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Amount = g.Sum(s => s.amount)
                    })
                    .ToList();

                return View("~/Views/finance/AdminRevenue.cshtml");
            }
            catch (Exception)
            {
                return View("~/Views/finance/AdminRevenue.cshtml");
            }
        }

        // 
        //  CUSTOMER PAYMENT HISTORY SECTION
        // 
        public IActionResult CustomerPaymentHistory()
        {
            try
            {
                int targetId = 1; // Customer ID 1

                
                var history = _context.payments
                    .FromSqlRaw("SELECT * FROM payments WHERE customer_id = {0} ORDER BY payment_id DESC", targetId)
                    .AsNoTracking()
                    .ToList();

                return View("~/Views/finance/CustomerPaymentHistory.cshtml", history);
            }
            catch (Exception ex)
            {
                ViewBag.DebugInfo = "Error: " + ex.Message;
                return View("~/Views/finance/CustomerPaymentHistory.cshtml", new List<Payment>());
            }
        }
    }
}