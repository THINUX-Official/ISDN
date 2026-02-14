using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int order_id { get; set; }

        // RDC Filter 
        [Column("rdc_id")]
        public int? rdc_id { get; set; }

        [Column("user_id")]
        public int user_id { get; set; }

        [Column("customer_id")]
        public int? customer_id { get; set; }

        [Column("order_number")]
        public string order_number { get; set; }

        [Column("order_date")]
        public DateTime order_date { get; set; }

        [Column("total_amount")]
        public decimal total_amount { get; set; }

        [Column("status")]
        public string status { get; set; }

        // Relationship with OrderItems
        public List<OrderItem> OrderItems { get; set; }
    }
}