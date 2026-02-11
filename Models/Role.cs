using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDN.Models
{
    /// <summary>
    /// Role entity mapped to existing 'roles' table in MySQL
    /// </summary>
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required]
        [Column("role_name")]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Column("parent_role_id")]
        public int? ParentRoleId { get; set; }

        // Navigation properties
        [ForeignKey("ParentRoleId")]
        public virtual Role? ParentRole { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
