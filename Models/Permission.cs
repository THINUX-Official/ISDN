using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// Permission entity mapped to existing 'permissions' table in MySQL
    /// </summary>
    [Table("permissions")]
    public class Permission
    {
        [Key]
        [Column("permission_id")]
        public int PermissionId { get; set; }

        [Required]
        [Column("permission_name")]
        [MaxLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(255)]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
