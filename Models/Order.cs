using System.ComponentModel.DataAnnotations;

namespace ISDN.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
