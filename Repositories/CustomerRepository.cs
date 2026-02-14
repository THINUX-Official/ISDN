using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;



namespace ISDN.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int customerId);
        Task<Customer?> GetByUserIdAsync(int userId);
        Task<Customer?> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<IEnumerable<Customer>> GetByRdcIdAsync(int rdcId);
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int customerId);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IsdnDbContext _context;

        public CustomerRepository(IsdnDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            return await _context.Customers
                .Include(c => c.Rdc)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await _context.Customers
                .Include(c => c.Rdc)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            // මචං මෙතන පරණ 'Email' වෙනුවට අලුත් 'email' property එක දැම්මා
            return await _context.Customers
                .Include(c => c.Rdc)
                .FirstOrDefaultAsync(c => c.email == email);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            // 'CustomerName' වෙනුවට 'first_name' එකෙන් order කරනවා
            return await _context.Customers
                .Include(c => c.Rdc)
                .OrderBy(c => c.first_name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetByRdcIdAsync(int rdcId)
        {
            // 'CustomerName' වෙනුවට 'first_name' එකෙන් order කරනවා
            return await _context.Customers
                .Include(c => c.Rdc)
                .Where(c => c.RdcId == rdcId)
                .OrderBy(c => c.first_name)
                .ToListAsync();
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}
