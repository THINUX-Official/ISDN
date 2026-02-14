using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// User entity mapped to existing 'users' table in MySQL
    /// </summary>
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("full_name")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("two_factor_enabled")]
        public bool TwoFactorEnabled { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        // Navigation property
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }

        public virtual ICollection<JwtToken> JwtTokens { get; set; } = new List<JwtToken>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
