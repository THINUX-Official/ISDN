using ISDN.Models;

namespace ISDN.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<Product?> GetByIdAsync(int id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}
