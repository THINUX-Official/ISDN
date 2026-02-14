using Microsoft.AspNetCore.Mvc;
using ISDN.Models;
using ISDN.Interfaces;
using System;

namespace ISDN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository _repo;

        public PaymentController(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index(string productName, decimal totalPrice, int? orderId)
        {
            var customer = _repo.GetFirstCustomer();
            if (customer != null)
            {
                ViewBag.FirstName = customer.first_name;
                ViewBag.LastName = customer.last_name;
                ViewBag.Address = customer.street_address;
                ViewBag.ZipCode = customer.zip_code;
                ViewBag.Phone = customer.phone_number;
            }

            ViewBag.ProductName = productName ?? "Asus Vivobook X";
            ViewBag.TotalPrice = totalPrice > 0 ? totalPrice : 262900.00m;
            ViewBag.OrderId = orderId ?? 1;

            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(Payment payment, string firstName, string lastName, string address, string phone_num, string payment_method, string bank_ref)
        {
            try
            {
                // 
                if (payment_method == "Card")
                {
                    payment.payment_method = "Credit/Debit Card"; // Database 

                    if (!string.IsNullOrEmpty(payment.card_number) && payment.card_number.Length == 16)
                    {
                        payment.card_number = "############" + payment.card_number.Substring(12);
                    }
                    payment.exp_month = "##";
                    payment.exp_year = "##";
                    payment.cvc = "###";
                    payment.payment_status = "Paid";
                }
                else
                {
                    payment.payment_method = "Direct Bank Transfer"; // Database
                    payment.card_name = "Bank Transfer";
                    payment.card_number = "REF: " + bank_ref;
                    payment.exp_month = "--";
                    payment.exp_year = "--";
                    payment.cvc = "---";
                    payment.payment_status = "Pending";
                }

                payment.transaction_id = "TXN" + DateTime.Now.Ticks.ToString().Substring(10);
                payment.payment_date = DateTime.Now;

                _repo.SavePayment(payment);

                return RedirectToAction("PaymentSuccess", new
                {
                    custName = firstName + " " + lastName,
                    amount = payment.amount,
                    orderId = payment.order_id,
                    addr = address,
                    phone = phone_num
                });
            }
            catch (Exception ex) { return Content("Error: " + ex.Message); }
        }

        public IActionResult PaymentSuccess(string custName, decimal amount, int orderId, string addr, string phone)
        {
            ViewBag.CustomerName = custName;
            ViewBag.TotalPrice = amount;
            ViewBag.OrderId = orderId;
            ViewBag.Address = addr;
            ViewBag.Phone = phone;
            ViewBag.ProductName = "Asus Vivobook X";
            return View();
        }
    }
}