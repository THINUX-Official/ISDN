using ISDN.Models;

namespace ISDN_Distribution.Models
{
    public class LogisticsScheduleViewModel
    {
        public List<Order> ReceivedForDeliveryOrders { get; set; } = new();
        public List<Order> OnTheWayOrders { get; set; } = new();
        public List<User> ActiveDrivers { get; set; } = new();
    }
}
