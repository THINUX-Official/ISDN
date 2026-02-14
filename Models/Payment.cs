using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int payment_id { get; set; }

        [Column("order_id")]
        public int? order_id { get; set; }

        [Column("customer_id")]
        public int? customer_id { get; set; }

        [Column("rdc_id")]
        public int? rdc_id { get; set; }

        [Column("amount")]
        public decimal amount { get; set; }

        [Column("payment_method")]
        public string? payment_method { get; set; }

        [Column("payment_status")]
        public string? payment_status { get; set; }

        [Column("transaction_id")]
        public string? transaction_id { get; set; } // Transaction ID

        [Column("payment_date")]
        public DateTime? payment_date { get; set; }

        // Dashboard code
        [Column("card_name")] public string? card_name { get; set; }
        [Column("card_number")] public string? card_number { get; set; }
        [Column("exp_month")] public string? exp_month { get; set; }
        [Column("exp_year")] public string? exp_year { get; set; }
        [Column("cvc")] public string? cvc { get; set; }
    }
}