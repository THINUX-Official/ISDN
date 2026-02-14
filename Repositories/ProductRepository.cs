using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;

namespace ISDN.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int productId);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int productId);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly IsdnDbContext _context;

        public ProductRepository(IsdnDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Inventories)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Inventories)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Inventories)
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
