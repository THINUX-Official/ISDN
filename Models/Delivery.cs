using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// Delivery entity mapped to existing 'deliveries' table in MySQL
    /// </summary>
    [Table("deliveries")]
    public class Delivery
    {
        [Key]
        [Column("delivery_id")]
        public int DeliveryId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Column("driver_id")]
        public int? DriverId { get; set; }

        [Column("scheduled_date")]
        public DateTime? ScheduledDate { get; set; }

        [Column("delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Column("tracking_number")]
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [Column("notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("DriverId")]
        public virtual User? Driver { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }
    }
}
