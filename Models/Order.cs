using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Models
{
    /// <summary>
    /// Order entity mapped to existing 'orders' table in MySQL
    /// </summary>
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Required]
        [Column("order_number")]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Column("delivery_address")]
        [MaxLength(255)]
        public string? DeliveryAddress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
