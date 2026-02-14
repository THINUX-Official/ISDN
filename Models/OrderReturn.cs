using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("order_returns")]
    public class OrderReturn
    {
        [Key]
        public int return_id { get; set; }
        public int? order_id { get; set; }
        public int? product_id { get; set; }
        public int? quantity { get; set; }
        public string? reason { get; set; }
        public string? refund_status { get; set; }
        public int? processed_by_id { get; set; }
        public DateTime? created_at { get; set; }
        public string? return_type { get; set; }
        public string? admin_comment { get; set; }
        public int? reason_id { get; set; }
        public string? other_reason_description { get; set; }

        [Column("refund_amount")]
        public decimal refund_amount { get; set; }

        public string? status { get; set; }
    }
}