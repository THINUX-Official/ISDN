using System;

namespace ISDN.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ProductImageUrl { get; set; }

        public decimal Total => UnitPrice * Quantity;
    }
}
