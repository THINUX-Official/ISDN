using ISDN.Constants;
using ISDN.Models;
using ISDN.Services;
using Microsoft.EntityFrameworkCore;

namespace ISDN.Data
{
    /// <summary>
    /// Database initializer to seed roles and default users
    /// </summary>
    public static class IsdnDbInitializer
    {
        public static async Task SeedDataAsync(IsdnDbContext context, IAuditLogService auditService)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed RDCs first (required for users)
            if (!await context.Rdcs.AnyAsync())
            {
                var rdcs = new List<Rdc>
                {
                    new Rdc { RdcId = 1, RdcName = "North RDC", RdcCode = "NORTH", Region = "Northern", Address = "Jaffna", ContactNumber = "+94771234501", IsActive = true },
                    new Rdc { RdcId = 2, RdcName = "South RDC", RdcCode = "SOUTH", Region = "Southern", Address = "Galle", ContactNumber = "+94771234502", IsActive = true },
                    new Rdc { RdcId = 3, RdcName = "East RDC", RdcCode = "EAST", Region = "Eastern", Address = "Batticaloa", ContactNumber = "+94771234503", IsActive = true },
                    new Rdc { RdcId = 4, RdcName = "West RDC", RdcCode = "WEST", Region = "Western", Address = "Colombo", ContactNumber = "+94771234504", IsActive = true },
                    new Rdc { RdcId = 5, RdcName = "Central RDC", RdcCode = "CENTRAL", Region = "Central", Address = "Kandy", ContactNumber = "+94771234505", IsActive = true }
                };

                await context.Rdcs.AddRangeAsync(rdcs);
                await context.SaveChangesAsync();
            }

            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role { RoleName = UserRoles.Admin, ParentRoleId = null },
                    new Role { RoleName = UserRoles.HeadOffice, ParentRoleId = null },
                    new Role { RoleName = UserRoles.RdcStaff, ParentRoleId = null },
                    new Role { RoleName = UserRoles.Logistics, ParentRoleId = null },
                    new Role { RoleName = UserRoles.Driver, ParentRoleId = null },
                    new Role { RoleName = UserRoles.Finance, ParentRoleId = null },
                    new Role { RoleName = UserRoles.SalesRep, ParentRoleId = null },
                    new Role { RoleName = UserRoles.Customer, ParentRoleId = null }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed Default Admin User
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == UserRoles.Admin);
                if (adminRole != null)
                {
                    var adminUser = new User
                    {
                        FullName = "System Administrator",
                        Email = "admin@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        RoleId = adminRole.RoleId,
                        RdcId = null, // Head Office
                        IsActive = true,
                        TwoFactorEnabled = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();

                    // Log admin creation
                    await auditService.LogActionAsync(adminUser.UserId, "ADMIN_CREATED", "User", 
                        adminUser.UserId, "Default admin user created", "System");
                }

                // Create test users for each role
                var roles = await context.Roles.ToListAsync();
                
                var testUsers = new List<User>
                {
                    // Head Office Users (rdc_id = NULL)
                    new User
                    {
                        FullName = "Head Office Manager",
                        Email = "headoffice@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("HeadOffice@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.HeadOffice).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FullName = "RDC Staff Member (Unassigned)",
                        Email = "rdc@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("RdcStaff@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.RdcStaff).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FullName = "Logistics Coordinator (Unassigned)",
                        Email = "logistics@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Logistics@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.Logistics).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FullName = "Delivery Driver (Unassigned)",
                        Email = "driver@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Driver@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.Driver).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FullName = "Finance Officer (Unassigned)",
                        Email = "finance@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Finance@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.Finance).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FullName = "Sales Representative (Unassigned)",
                        Email = "sales@isdn.lk",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sales@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.SalesRep).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        FullName = "Test Customer",
                        Email = "customer@test.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                        RoleId = roles.First(r => r.RoleName == UserRoles.Customer).RoleId,
                        RdcId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                // Standardized RDC staff structure - every RDC has all 5 operational roles for consistency
                
                var rdcStaffRole = roles.First(r => r.RoleName == UserRoles.RdcStaff).RoleId;
                var logisticsRole = roles.First(r => r.RoleName == UserRoles.Logistics).RoleId;
                var driverRole = roles.First(r => r.RoleName == UserRoles.Driver).RoleId;
                var financeRole = roles.First(r => r.RoleName == UserRoles.Finance).RoleId;
                var salesRepRole = roles.First(r => r.RoleName == UserRoles.SalesRep).RoleId;

                // NORTH RDC (rdc_id = 1) - Complete Staff
                testUsers.AddRange(new List<User>
                {
                    new User { FullName = "North RDC Staff", Email = "north-rdc@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("NorthRDC@123"), 
                              RoleId = rdcStaffRole, RdcId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "North Logistics", Email = "north-logistics@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("NorthLogistics@123"), 
                              RoleId = logisticsRole, RdcId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "North Driver", Email = "north-driver@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("NorthDriver@123"), 
                              RoleId = driverRole, RdcId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "North Finance", Email = "north-finance@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("NorthFinance@123"), 
                              RoleId = financeRole, RdcId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "North Sales Rep", Email = "north-sales@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("NorthSales@123"), 
                              RoleId = salesRepRole, RdcId = 1, IsActive = true, CreatedAt = DateTime.UtcNow }
                });

                // SOUTH RDC (rdc_id = 2) - Complete Staff
                testUsers.AddRange(new List<User>
                {
                    new User { FullName = "South RDC Staff", Email = "south-rdc@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("SouthRDC@123"), 
                              RoleId = rdcStaffRole, RdcId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "South Logistics", Email = "south-logistics@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("SouthLogistics@123"), 
                              RoleId = logisticsRole, RdcId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "South Driver", Email = "south-driver@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("SouthDriver@123"), 
                              RoleId = driverRole, RdcId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "South Finance", Email = "south-finance@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("SouthFinance@123"), 
                              RoleId = financeRole, RdcId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "South Sales Rep", Email = "south-sales@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("SouthSales@123"), 
                              RoleId = salesRepRole, RdcId = 2, IsActive = true, CreatedAt = DateTime.UtcNow }
                });

                // EAST RDC (rdc_id = 3) - Complete Staff
                testUsers.AddRange(new List<User>
                {
                    new User { FullName = "East RDC Staff", Email = "east-rdc@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("EastRDC@123"), 
                              RoleId = rdcStaffRole, RdcId = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "East Logistics", Email = "east-logistics@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("EastLogistics@123"), 
                              RoleId = logisticsRole, RdcId = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "East Driver", Email = "east-driver@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("EastDriver@123"), 
                              RoleId = driverRole, RdcId = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "East Finance", Email = "east-finance@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("EastFinance@123"), 
                              RoleId = financeRole, RdcId = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "East Sales Rep", Email = "east-sales@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("EastSales@123"), 
                              RoleId = salesRepRole, RdcId = 3, IsActive = true, CreatedAt = DateTime.UtcNow }
                });

                // WEST RDC (rdc_id = 4) - Complete Staff
                testUsers.AddRange(new List<User>
                {
                    new User { FullName = "West RDC Staff", Email = "west-rdc@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("WestRDC@123"), 
                              RoleId = rdcStaffRole, RdcId = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "West Logistics", Email = "west-logistics@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("WestLogistics@123"), 
                              RoleId = logisticsRole, RdcId = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "West Driver", Email = "west-driver@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("WestDriver@123"), 
                              RoleId = driverRole, RdcId = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "West Finance", Email = "west-finance@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("WestFinance@123"), 
                              RoleId = financeRole, RdcId = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "West Sales Rep", Email = "west-sales@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("WestSales@123"), 
                              RoleId = salesRepRole, RdcId = 4, IsActive = true, CreatedAt = DateTime.UtcNow }
                });

                // CENTRAL RDC (rdc_id = 5) - Complete Staff
                testUsers.AddRange(new List<User>
                {
                    new User { FullName = "Central RDC Staff", Email = "central-rdc@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("CentralRDC@123"), 
                              RoleId = rdcStaffRole, RdcId = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "Central Logistics", Email = "central-logistics@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("CentralLogistics@123"), 
                              RoleId = logisticsRole, RdcId = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "Central Driver", Email = "central-driver@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("CentralDriver@123"), 
                              RoleId = driverRole, RdcId = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "Central Finance", Email = "central-finance@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("CentralFinance@123"), 
                              RoleId = financeRole, RdcId = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { FullName = "Central Sales Rep", Email = "central-sales@isdn.lk", 
                              PasswordHash = BCrypt.Net.BCrypt.HashPassword("CentralSales@123"), 
                              RoleId = salesRepRole, RdcId = 5, IsActive = true, CreatedAt = DateTime.UtcNow }
                });

                await context.Users.AddRangeAsync(testUsers);
                await context.SaveChangesAsync();
            }

            // Seed Sample Products
            if (!await context.Products.AnyAsync())
            {
                var products = new List<Product>
                {
                    new Product { ProductName = "Rice - 5kg", Description = "Premium quality rice", Sku = "RICE-5KG", UnitPrice = 850.00m, Category = "Grains", IsActive = true },
                    new Product { ProductName = "Sugar - 1kg", Description = "White refined sugar", Sku = "SUGAR-1KG", UnitPrice = 150.00m, Category = "Groceries", IsActive = true },
                    new Product { ProductName = "Flour - 1kg", Description = "All-purpose wheat flour", Sku = "FLOUR-1KG", UnitPrice = 120.00m, Category = "Groceries", IsActive = true },
                    new Product { ProductName = "Cooking Oil - 1L", Description = "Vegetable cooking oil", Sku = "OIL-1L", UnitPrice = 450.00m, Category = "Oils", IsActive = true },
                    new Product { ProductName = "Tea - 200g", Description = "Premium Ceylon tea", Sku = "TEA-200G", UnitPrice = 380.00m, Category = "Beverages", IsActive = true }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                // Add inventory for products
                var inventories = products.Select(p => new Inventory
                {
                    ProductId = p.ProductId,
                    Location = "Main Warehouse",
                    QuantityAvailable = 1000,
                    QuantityReserved = 0,
                    ReorderLevel = 100,
                    LastUpdated = DateTime.UtcNow
                }).ToList();

                await context.Inventories.AddRangeAsync(inventories);
                await context.SaveChangesAsync();
            }

            // Seed Permissions (optional - can be expanded)
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    new Permission { PermissionName = "VIEW_USERS", Description = "View user list" },
                    new Permission { PermissionName = "CREATE_USERS", Description = "Create new users" },
                    new Permission { PermissionName = "EDIT_USERS", Description = "Edit existing users" },
                    new Permission { PermissionName = "DELETE_USERS", Description = "Delete users" },
                    new Permission { PermissionName = "VIEW_PRODUCTS", Description = "View product catalog" },
                    new Permission { PermissionName = "MANAGE_PRODUCTS", Description = "Create/Edit/Delete products" },
                    new Permission { PermissionName = "VIEW_ORDERS", Description = "View orders" },
                    new Permission { PermissionName = "PROCESS_ORDERS", Description = "Process and manage orders" },
                    new Permission { PermissionName = "VIEW_INVENTORY", Description = "View inventory levels" },
                    new Permission { PermissionName = "MANAGE_INVENTORY", Description = "Update inventory levels" },
                    new Permission { PermissionName = "VIEW_DELIVERIES", Description = "View delivery schedules" },
                    new Permission { PermissionName = "MANAGE_DELIVERIES", Description = "Schedule and manage deliveries" },
                    new Permission { PermissionName = "VIEW_PAYMENTS", Description = "View payment records" },
                    new Permission { PermissionName = "PROCESS_PAYMENTS", Description = "Process payments" },
                    new Permission { PermissionName = "VIEW_REPORTS", Description = "View system reports" },
                    new Permission { PermissionName = "VIEW_AUDIT_LOGS", Description = "View audit logs" }
                };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
