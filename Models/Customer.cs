using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// Customer entity mapped to existing 'customers' table in MySQL
    /// Separate from Users - customers have their own regional assignment
    /// </summary>
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [Column("customer_name")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Column("email")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("address")]
        [MaxLength(255)]
        public string? Address { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
