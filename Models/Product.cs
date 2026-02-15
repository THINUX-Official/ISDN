using System.ComponentModel.DataAnnotations;

namespace ISDN.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(255)]
        public string? ImageRelativePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
