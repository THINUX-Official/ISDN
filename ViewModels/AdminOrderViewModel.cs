using ISDN.Models;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

using System.Collections.Generic;
using System.Linq;

using System.Collections.Generic;
using System.Linq;
// Order සහ OrderReturn models තිබෙන තැන අනුව මෙය වෙනස් විය හැක

namespace ISDN_Distribution.Models
{
    public class AdminOrderViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderReturn> ActiveReturns { get; set; } = new List<OrderReturn>();

        // මේ properties දෙක අනිවාර්යයෙන් තියෙන්න ඕනේ Repository එකේ assign කරන නිසා
        public bool IsStockAvailable { get; set; } = true;
        public bool IsPacked { get; set; }

        public bool IsPendingConfirmation => Order.Status == "Placed" && !ActiveReturns.Any();
        public bool IsPackedAndActive => Order.Status == "Packed" && !ActiveReturns.Any();
        public bool HasPendingReturn => ActiveReturns.Any();
    }
}