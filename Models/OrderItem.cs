using System.ComponentModel.DataAnnotations;

namespace ISDN.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Subtotal { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
