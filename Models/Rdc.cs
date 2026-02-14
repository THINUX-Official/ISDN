using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// Regional Distribution Centre reference table
    /// Maps to existing 'rdcs' table in MySQL
    /// </summary>
    [Table("rdcs")]
    public class Rdc
    {
        [Key]
        [Column("rdc_id")]
        public int RdcId { get; set; }

        [Required]
        [Column("rdc_name")]
        [MaxLength(50)]
        public string RdcName { get; set; } = string.Empty;

        [Column("rdc_code")]
        [MaxLength(10)]
        public string? RdcCode { get; set; }

        [Column("region")]
        [MaxLength(50)]
        public string? Region { get; set; }

        [Column("address")]
        [MaxLength(255)]
        public string? Address { get; set; }

        [Column("contact_number")]
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
