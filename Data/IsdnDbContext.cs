using Microsoft.EntityFrameworkCore;
using ISDN.Models;

namespace ISDN.Data
{
    /// <summary>
    /// IsdnDbContext is the Entity Framework Core database context for the ISDN Distribution Management System.
    /// Maps to the existing MySQL database 'isdn_distribution_db'.
    /// </summary>
    public class IsdnDbContext : DbContext
    {
        public IsdnDbContext(DbContextOptions<IsdnDbContext> options) : base(options)
        {
        }

        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<JwtToken> JwtTokens { get; set; }

        // Regional Distribution Centres
        public DbSet<Rdc> Rdcs { get; set; }
        public DbSet<Customer> Customers { get; set; }

        // Products & Inventory
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        // Orders & Deliveries
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<ReturnReason> ReturnReasons { get; set; }


      


        public DbSet<OrderReturn> OrderReturns { get; set; }
        public DbSet<OrderStatusLog> OrderStatusLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints

            // Configure decimal precision for MySQL
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(10,2)");

            

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Subtotal)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10,2)");

            // User - Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - RDC relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Rdc)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RdcId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer - RDC relationship
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Rdc)
                .WithMany(r => r.Customers)
                .HasForeignKey(c => c.RdcId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer - User relationship (optional link)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Role - ParentRole relationship (hierarchical)
            modelBuilder.Entity<Role>()
                .HasOne(r => r.ParentRole)
                .WithMany()
                .HasForeignKey(r => r.ParentRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // RolePermission relationships
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - Inventory relationship
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Inventory - RDC relationship
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Rdc)
                .WithMany(r => r.Inventories)
                .HasForeignKey(i => i.RdcId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - Customer relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - RDC relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Rdc)
                .WithMany(r => r.Orders)
                .HasForeignKey(o => o.RdcId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Delivery relationships
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithMany(o => o.Deliveries)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Driver)
                .WithMany()
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Delivery - RDC relationship
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Rdc)
                .WithMany()
                .HasForeignKey(d => d.RdcId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment - RDC relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Rdc)
                .WithMany()
                .HasForeignKey(p => p.RdcId)
                .OnDelete(DeleteBehavior.Restrict);

            // JWT Token relationships
            modelBuilder.Entity<JwtToken>()
                .HasOne(t => t.User)
                .WithMany(u => u.JwtTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit Log relationships
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            modelBuilder.Entity<Customer>()
        .Property(c => c.registration_status)
        .HasConversion<string>(); // MySQL Enum එක string එකක් විදිහට map කරන්න

            // Boolean mapping (tinyint(1) mapping එක ස්ථිර කරන්න)
            modelBuilder.Entity<Customer>()
                .Property(c => c.IsActive)
                .HasColumnType("tinyint(1)");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasColumnType("tinyint(1)");
        }
    }
}
