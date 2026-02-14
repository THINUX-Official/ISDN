using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("order_items")] // MySQL 
    public class OrderItem
    {
        [Key]
        public int order_item_id { get; set; }

        public int order_id { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }

        // --- connection---
        [ForeignKey("order_id")]
        public Order Order { get; set; }

        [ForeignKey("product_id")]
        public Product Product { get; set; }
    }
}