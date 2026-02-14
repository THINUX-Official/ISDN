using ISDN_Distribution.Models;
using System.Threading.Tasks;

namespace ISDN_Distribution.Repositories
{
    public interface IOrderRepository
    {
        // Method දෙකම එකතු කරමු එවිට පරණ Controller errors මැකී යනු ඇත
        Task<CustomerOrdersViewModel> GetCustomerOrdersAsync(int userId);
        Task<CustomerOrdersViewModel> GetByUserIdAsync(int userId);
    }
}
