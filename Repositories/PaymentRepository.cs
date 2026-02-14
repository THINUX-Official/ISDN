using ISDN.Models;
using ISDN.Interfaces;
using System.Linq;

namespace ISDN.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public Customer GetFirstCustomer()
        {
            // Database 
            return _context.customers.FirstOrDefault();
        }

        public void SavePayment(Payment payment)
        {
            _context.payments.Add(payment);
            _context.SaveChanges();
        }
    }
}