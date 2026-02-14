using Microsoft.EntityFrameworkCore;
using ISDN.Models.Entities;

namespace ISDN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Rdc> Rdcs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match MySQL database
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Rdc>().ToTable("rdcs");
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<OrderItem>().ToTable("order_items");
            modelBuilder.Entity<Inventory>().ToTable("inventories");
            modelBuilder.Entity<Delivery>().ToTable("deliveries");
            modelBuilder.Entity<Invoice>().ToTable("invoices");
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<AuditLog>().ToTable("audit_logs");

            // Configure primary keys
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Role>().HasKey(r => r.RoleId);
            modelBuilder.Entity<Rdc>().HasKey(r => r.RdcId);
            modelBuilder.Entity<Product>().HasKey(p => p.ProductId);
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderItem>().HasKey(oi => oi.OrderItemId);
            modelBuilder.Entity<Inventory>().HasKey(i => i.InventoryId);
            modelBuilder.Entity<Delivery>().HasKey(d => d.DeliveryId);
            modelBuilder.Entity<Invoice>().HasKey(i => i.InvoiceId);
            modelBuilder.Entity<Payment>().HasKey(p => p.PaymentId);
            modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId);
            modelBuilder.Entity<AuditLog>().HasKey(a => a.AuditId);

            // Configure column names to match MySQL snake_case convention
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RdcId).HasColumnName("rdc_id");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.RdcId).HasColumnName("rdc_id");
                entity.Property(e => e.OrderDate).HasColumnName("order_date");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
                entity.Property(e => e.DeliveryAddress).HasColumnName("delivery_address");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
                entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.ProductName).HasColumnName("product_name");
                entity.Property(e => e.Category).HasColumnName("category");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
                entity.Property(e => e.RdcId).HasColumnName("rdc_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.QtyOnHand).HasColumnName("qty_on_hand");
                entity.Property(e => e.ReorderLevel).HasColumnName("reorder_level");
                entity.Property(e => e.LastRestocked).HasColumnName("last_restocked");
            });
        }
    }
}
