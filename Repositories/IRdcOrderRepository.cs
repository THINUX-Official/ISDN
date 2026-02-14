using ISDN_Distribution.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISDN_Distribution.Repositories
{
    public interface IRdcOrderRepository
    {
        // RDC එකට අදාළ සියලුම orders සහ return requests ලබා ගැනීම
        Task<List<AdminOrderViewModel>> GetOrdersByRdcAsync(int rdcId);

        // Order status එක (Placed -> Packed) update කිරීම
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, int adminId);

        // Refund request එකක් Approve හෝ Reject කිරීම
        Task<bool> ProcessReturnAsync(int returnId, string status, string adminComment, int adminId);
    }
}
