using ISDN.Models;
using ISDN_Distribution.Models;
using System.Threading.Tasks;

namespace ISDN_Distribution.Repositories
{
    public interface IOrderRepository
    {
        // Method දෙකම එකතු කරමු එවිට පරණ Controller errors මැකී යනු ඇත
        Task<CustomerOrdersViewModel> GetCustomerOrdersAsync(int userId);
        Task<CustomerOrdersViewModel> GetByUserIdAsync(int userId);
        Task<CustomerOrdersViewModel> GetClusterOrdersAsync(int userId, int? branchId = null, string? businessType = null);

        Task<List<Delivery>> GetDriverTasksAsync(int driverId, int rdcId);

        Task<List<Order>> GetOrdersByStatusAndRdcAsync(string status, int rdcId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, int userId);
        Task<List<User>> GetActiveDriversByRdcAsync(int rdcId);
        Task<bool> AssignDriverAndDispatchAsync(int orderId, int driverId, int userId);
    }
}
