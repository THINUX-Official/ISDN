using System.ComponentModel.DataAnnotations;

namespace ISDN.Models
{
    public class Product
    {
        [Key]
        public int product_id { get; set; } // Database 

        public string product_name { get; set; }

        public decimal unit_price { get; set; }

        // 
        public decimal manufacturer_price { get; set; }
    }
}