using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// Inventory entity mapped to existing 'inventory' table in MySQL
    /// </summary>
    [Table("inventory")]
    public class Inventory
    {
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Column("location")]
        [MaxLength(100)]
        public string? Location { get; set; }

        [Column("quantity_available")]
        public int QuantityAvailable { get; set; }

        [Column("quantity_reserved")]
        public int QuantityReserved { get; set; }

        [Column("reorder_level")]
        public int ReorderLevel { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }
    }
}
