using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Models
{
    /// <summary>
    /// Product entity mapped to existing 'products' table in MySQL
    /// </summary>
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("product_name")]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("sku")]
        [MaxLength(50)]
        public string? Sku { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("category")]
        [MaxLength(50)]
        public string? Category { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
