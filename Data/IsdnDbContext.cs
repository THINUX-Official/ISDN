using Microsoft.EntityFrameworkCore;
using ISDN.Models;

namespace ISDN.Data
{
    public class IsdnDbContext : DbContext
    {
        public IsdnDbContext(DbContextOptions<IsdnDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity"); // Needs to be added to DB
                entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedDate).HasColumnName("created_at");
                entity.Property(e => e.ModifiedDate).HasColumnName("modified_date"); // Needs to be added to DB
                entity.Property(e => e.ImageRelativePath).HasColumnName("product_image_url").HasMaxLength(255);
            });

            // Configure Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.OrderNumber).HasColumnName("order_number").IsRequired().HasMaxLength(50);
                entity.Property(e => e.OrderDate).HasColumnName("order_date");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);

                entity.HasMany(e => e.OrderItems)
                      .WithOne(e => e.Order)
                      .HasForeignKey(e => e.OrderId);
            });

            // Configure OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(e => e.OrderItemId);
                entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId);
            });
        }
    }
}
