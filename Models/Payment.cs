using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Models
{
    /// <summary>
    /// Payment entity mapped to existing 'payments' table in MySQL
    /// </summary>
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("payment_method")]
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [Column("payment_status")]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [Column("transaction_id")]
        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }
    }
}
