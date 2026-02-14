using ISDN.Models;
using System;
using System.Collections.Generic;

namespace ISDN_Distribution.Models
{
    public class CustomerOrdersViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<string> ReturnReasons { get; set; } = new List<string>();

        // අන්න දැන් MyReturns හරියටම මෙතන තියෙනවා!
        public List<OrderReturn> MyReturns { get; set; } = new List<OrderReturn>();
    }

    public class OrderDisplayDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public bool IsReturnable => Status == "Delivered" && (DateTime.Now - OrderDate).TotalHours <= 72;
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}

