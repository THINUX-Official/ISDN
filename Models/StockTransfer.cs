using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("stock_transfers")]
    public class StockTransfer
    {
        [Key]
        [Column("transfer_id")]
        public int TransferId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("from_rdc_id")]
        public int FromRdcId { get; set; }

        [Column("to_rdc_id")]
        public int ToRdcId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } // PENDING, COMPLETED, etc.

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("FromRdcId")]
        public virtual Rdc? FromRdc { get; set; }

        [ForeignKey("ToRdcId")]
        public virtual Rdc? ToRdc { get; set; }
    }
}