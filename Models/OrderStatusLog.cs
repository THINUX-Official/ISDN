using ISDN.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("order_status_logs")]
public class OrderStatusLog
{
    [Key]
    [Column("status_log_id")]
    public int StatusLogId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("status")]
    public string Status { get; set; }

    [Column("updated_by_id")]
    public int UpdatedById { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Use the attribute here to link specifically to the 'OrderId' above
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
}