using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using ISDN.Models;

namespace ISDN.ViewModels
{
    public class RdcPackRequestViewModel
    {
        public List<OrderAssistanceDto> PendingOrders { get; set; } = new List<OrderAssistanceDto>();
        public List<SelectListItem> TargetRdcs { get; set; } = new List<SelectListItem>();
    }

    public class OrderAssistanceDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public List<OrderAssistanceItemDto> Items { get; set; } = new List<OrderAssistanceItemDto>();
    }

    public class OrderAssistanceItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}