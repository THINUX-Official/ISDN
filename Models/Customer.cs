using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ISDN.Models
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [Column("first_name")]
        [MaxLength(50)]
        public string first_name { get; set; } = string.Empty;

        [Required]
        [Column("last_name")]
        [MaxLength(50)]
        public string last_name { get; set; } = string.Empty;

        [Column("email")]
        [MaxLength(100)]
        public string? email { get; set; }

        [Column("phone_number")]
        [MaxLength(20)]
        public string? phone_number { get; set; }

        [Column("business_name")]
        [MaxLength(150)]
        public string? business_name { get; set; }

        [Column("street_address")]
        [MaxLength(255)]
        public string? street_address { get; set; }

        [Column("city")]
        [MaxLength(100)]
        public string? city { get; set; }

        [Column("zip_code")]
        [MaxLength(10)]
        public string? zip_code { get; set; }

        [Column("temp_password_hash")]
        [MaxLength(255)]
        public string? temp_password_hash { get; set; }

        [Column("rdc_id")]
        public int? RdcId { get; set; }

        [Column("registration_status")] // MySQL Enum mapping
        public string registration_status { get; set; } = "PENDING";

        [Column("is_active")]
        public bool IsActive { get; set; } = false;

        [Column("disapproved_at")]
        public DateTime? DisapprovedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("RdcId")]
        public virtual Rdc? Rdc { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
