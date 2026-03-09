using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    [Table("return_reasons")]
    public class ReturnReason
    {
        [Key]
        [Column("reason_id")]
        public int ReasonId { get; set; }

        [Column("reason_text")]
        [Required]
        [MaxLength(100)]
        public string ReasonText { get; set; } = string.Empty;
    }
}
