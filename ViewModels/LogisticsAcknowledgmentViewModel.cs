using ISDN.Models;

namespace ISDN_Distribution.Models
{
    public class LogisticsAcknowledgmentViewModel
    {
        public List<Order> PendingPackedOrders { get; set; } = new();
        public List<Order> RecentlyAcknowledgedOrders { get; set; } = new();
    }
}
