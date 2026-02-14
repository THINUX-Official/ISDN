using Microsoft.EntityFrameworkCore;
using ISDN.Models;

namespace ISDN.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Payment> payments { get; set; }
        public DbSet<Customer> customers { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<OrderItem> order_items { get; set; }
        public DbSet<OrderReturn> order_returns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table Mapping
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<Payment>().HasKey(p => p.payment_id);

            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<Customer>().HasKey(c => c.customer_id);

            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Product>().HasKey(pr => pr.product_id);

            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<Order>().HasKey(o => o.order_id);

            modelBuilder.Entity<OrderItem>().ToTable("order_items");
            modelBuilder.Entity<OrderItem>().HasKey(oi => oi.order_item_id);

            modelBuilder.Entity<OrderReturn>().ToTable("order_returns");
            modelBuilder.Entity<OrderReturn>().HasKey(or => or.return_id);

            
        }
    }
}