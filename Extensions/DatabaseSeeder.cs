using ISDN.Data;
using ISDN.Models;

namespace ISDN.Extensions
{
    public static class DatabaseSeeder
    {
        public static void SeedData(IsdnDbContext context)
        {
            // Check if data already exists
            if (context.Products.Any())
            {
                return; // Database has been seeded
            }

            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with 2.4GHz connectivity and long battery life",
                    Category = "Electronics",
                    UnitPrice = 29.99m,
                    StockQuantity = 50,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Mechanical Keyboard",
                    Description = "RGB backlit mechanical keyboard with blue switches",
                    Category = "Electronics",
                    UnitPrice = 89.99m,
                    StockQuantity = 30,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "USB-C Hub",
                    Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader",
                    Category = "Electronics",
                    UnitPrice = 45.99m,
                    StockQuantity = 25,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Laptop Stand",
                    Description = "Adjustable aluminum laptop stand for better ergonomics",
                    Category = "Accessories",
                    UnitPrice = 39.99m,
                    StockQuantity = 40,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Webcam HD 1080p",
                    Description = "Full HD webcam with built-in microphone and auto-focus",
                    Category = "Electronics",
                    UnitPrice = 69.99m,
                    StockQuantity = 20,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Desk Organizer",
                    Description = "Bamboo desk organizer with multiple compartments",
                    Category = "Office Supplies",
                    UnitPrice = 24.99m,
                    StockQuantity = 60,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "LED Desk Lamp",
                    Description = "Adjustable LED desk lamp with touch control and USB charging port",
                    Category = "Lighting",
                    UnitPrice = 34.99m,
                    StockQuantity = 35,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Noise Cancelling Headphones",
                    Description = "Over-ear headphones with active noise cancellation and 30-hour battery",
                    Category = "Electronics",
                    UnitPrice = 149.99m,
                    StockQuantity = 15,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Portable SSD 1TB",
                    Description = "Ultra-fast portable SSD with USB 3.2 Gen 2 interface",
                    Category = "Storage",
                    UnitPrice = 119.99m,
                    StockQuantity = 22,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Monitor Arm Mount",
                    Description = "Adjustable monitor arm mount for screens up to 32 inches",
                    Category = "Accessories",
                    UnitPrice = 79.99m,
                    StockQuantity = 18,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Wireless Charger",
                    Description = "Fast wireless charging pad compatible with Qi-enabled devices",
                    Category = "Electronics",
                    UnitPrice = 19.99m,
                    StockQuantity = 0,
                    IsActive = false
                },
                new Product
                {
                    ProductName = "Cable Management Kit",
                    Description = "Complete cable management solution with clips, sleeves, and ties",
                    Category = "Accessories",
                    UnitPrice = 14.99m,
                    StockQuantity = 75,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
