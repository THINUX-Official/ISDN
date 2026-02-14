using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// AuditLog entity mapped to existing 'audit_logs' table in MySQL
    /// Tracks all important actions in the system
    /// </summary>
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("audit_id")]
        public int AuditId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("action")]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Column("entity_type")]
        [MaxLength(50)]
        public string? EntityType { get; set; }

        [Column("entity_id")]
        public int? EntityId { get; set; }

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
