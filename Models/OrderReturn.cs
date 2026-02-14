using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("order_returns")]
    public class OrderReturn
    {
        [Key]
        [Column("return_id")]
        public int ReturnId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        // මෙතන තිබුණ [Column("reason")] එක අයින් කළා. 
        // නැත්නම් Entity Framework එක 'reason' කියලා column එකක් database එකේ හොයනවා.
        [NotMapped]
        public string? Reason => OtherReasonDescription;

        [Column("refund_status")]
        public string RefundStatus { get; set; } = "PENDING";

        [Column("processed_by_id")]
        public int? ProcessedById { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("return_type")]
        public string? ReturnType { get; set; }

        [Column("admin_comment")]
        public string? AdminComment { get; set; }

        [Column("admin_status")]
        public string AdminStatus { get; set; } = "PENDING";

        [Column("reason_id")]
        public int? ReasonId { get; set; }

        [Column("other_reason_description")]
        public string? OtherReasonDescription { get; set; }
    }
}
