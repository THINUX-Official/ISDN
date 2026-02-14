using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// RolePermission entity mapped to existing 'role_permissions' table in MySQL
    /// Maps many-to-many relationship between roles and permissions
    /// </summary>
    [Table("role_permissions")]
    public class RolePermission
    {
        [Key]
        [Column("role_permission_id")]
        public int RolePermissionId { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("permission_id")]
        public int PermissionId { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission? Permission { get; set; }
    }
}
