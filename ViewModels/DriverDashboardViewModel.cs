using ISDN.Models;
using System.Collections.Generic;

namespace ISDN.Models
{
    public class DriverDashboardViewModel
    {
      
        public IEnumerable<Delivery> ActiveDeliveries { get; set; }
        public IEnumerable<Delivery> CompletedDeliveries { get; set; }

     
        public int TodayCount { get; set; }
        public int PendingCount { get; set; }
        public int CompletedTodayCount { get; set; }

     
        public string InitialMapAddress { get; set; }
    }
}
