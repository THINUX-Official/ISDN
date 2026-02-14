using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// JwtToken entity mapped to existing 'jwt_tokens' table in MySQL
    /// Stores issued JWT tokens for tracking and revocation
    /// </summary>
    [Table("jwt_tokens")]
    public class JwtToken
    {
        [Key]
        [Column("token_id")]
        public int TokenId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("token")]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
