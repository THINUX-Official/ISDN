# ISDN Distribution Management System - Dashboard & Reporting Module

## Overview
This is Part 7 of the ISDN Sales Distribution Management System - the Management Dashboard and Reporting module converted from PHP to ASP.NET Core MVC.

## Features Implemented

### 1. Executive Dashboard (`/Dashboard/Index`)
- **Real-time Sales Metrics:**
  - Sales Today with growth percentage
  - Sales This Month with growth percentage
  - Pending Orders count

- **Analytics Panels:**
  - Orders by Status (Last 30 days)
  - Top 5 Best-Selling Products
  - Low Stock Alert
  - Recent Orders (Latest 10)

- **Role-Based Data Filtering:**
  - Admin/Head Office: View all data across all RDCs
  - RDC Staff/Logistics/Others: View only their assigned RDC data

### 2. Sales Report (`/Dashboard/SalesReport`)
- **Date Range Filtering:**
  - Filter orders by custom date range
  - Default: Current month

- **Summary Metrics:**
  - Total Revenue
  - Total Orders
  - Completed Orders
  - Average Order Value

- **Detailed Order List:**
  - Order ID, Date, Status, Amount
  - Sorted by date (newest first)

- **CSV Export:**
  - Export filtered data to CSV file
  - Downloadable report

### 3. Premium UI Design
- Modern glassmorphism design
- Gradient backgrounds with animations
- Responsive layout (mobile-friendly)
- Smooth transitions and hover effects
- Professional color scheme

## Database Integration

The module connects to your existing MySQL database (`isdn_distribution_db`) and uses these tables:

- `orders` - Order data
- `order_items` - Order line items
- `products` - Product information
- `inventories` - Stock levels
- `users` - User authentication
- `roles` - User roles
- `rdcs` - Regional Distribution Centers

## File Structure

```
ISDN/
├── Controllers/
│   └── DashboardController.cs          # Main controller with all business logic
├── Models/
│   ├── DashboardViewModel.cs           # Dashboard data model
│   ├── SalesReportViewModel.cs         # Sales report data model
│   └── Entities/
│       └── DatabaseEntities.cs         # Database entity models
├── Views/
│   └── Dashboard/
│       ├── Index.cshtml                # Dashboard view
│       └── SalesReport.cshtml          # Sales report view
├── Data/
│   └── ApplicationDbContext.cs         # EF Core DbContext
├── appsettings.json                    # Configuration (update connection string)
└── Program.cs                          # App configuration
```

## Setup Instructions

### 1. Prerequisites
- .NET 8.0 SDK or later
- MySQL Server 8.0 or later
- Your existing ISDN database already set up

### 2. Update Connection String
Edit `appsettings.json` and update the MySQL connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=isdn_distribution_db;User=root;Password=YOUR_PASSWORD;SslMode=none;"
}
```

### 3. Install NuGet Packages

```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.Cookies
```

### 4. Run the Application

```bash
dotnet restore
dotnet build
dotnet run
```

### 5. Access the Dashboard

Navigate to:
- Dashboard: `https://localhost:5001/Dashboard/Index`
- Sales Report: `https://localhost:5001/Dashboard/SalesReport`

## Key Differences from PHP Demo

### Removed Demo Data
The PHP version used demo tables (`demo_orders`, `demo_inventory`, etc.). This .NET version uses your **actual production tables**:
- `demo_orders` → `orders`
- `demo_order_items` → `order_items`
- `demo_inventory` → `inventories`

### Added Features
1. **Role-Based Access Control**
   - Filters data based on user's RDC assignment
   - Admin/Head Office sees all data
   - RDC staff sees only their RDC data

2. **Type Safety**
   - Strongly-typed models
   - Compile-time error checking
   - Better IntelliSense support

3. **Entity Framework Core**
   - LINQ queries instead of raw SQL
   - Built-in SQL injection protection
   - Easier maintenance

4. **Async/Await**
   - Non-blocking database operations
   - Better performance under load

## Controller Methods

### DashboardController

```csharp
// Main dashboard with all metrics
public async Task<IActionResult> Index()

// Sales report with date filtering
public async Task<IActionResult> SalesReport(DateTime? from, DateTime? to)

// Export sales data to CSV
public async Task<IActionResult> ExportSalesReport(DateTime? from, DateTime? to)
```

## Database Queries Explained

### Sales Today
```csharp
var salesToday = await _context.Orders
    .Where(o => o.OrderDate.Date == today 
        && (o.Status == "DELIVERED" || o.Status == "COMPLETED"))
    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
```

### Top Products (Last 30 days)
```csharp
model.TopProducts = await (
    from oi in _context.OrderItems
    join o in _context.Orders on oi.OrderId equals o.OrderId
    join p in _context.Products on oi.ProductId equals p.ProductId
    where o.OrderDate >= thirtyDaysAgo
        && (o.Status == "DELIVERED" || o.Status == "COMPLETED")
    group oi by p.ProductName into g
    select new TopProduct
    {
        ProductName = g.Key,
        TotalQuantity = g.Sum(x => x.Quantity)
    })
    .OrderByDescending(tp => tp.TotalQuantity)
    .Take(5)
    .ToListAsync();
```

## Authentication Integration

The dashboard expects user claims for:
- `Name` - User's full name
- `Role` - User's role name
- `RdcId` - User's assigned RDC (optional)

Add these claims when user logs in:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.FullName),
    new Claim("Role", role.RoleName),
    new Claim("RdcId", user.RdcId?.ToString() ?? "")
};
```

## Customization

### Change Growth Percentages
Currently using mock values. To calculate real growth:

```csharp
// Calculate yesterday's sales
var yesterday = today.AddDays(-1);
var salesYesterday = await _context.Orders
    .Where(o => o.OrderDate.Date == yesterday 
        && (o.Status == "DELIVERED" || o.Status == "COMPLETED"))
    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

// Calculate growth
model.GrowthToday = salesYesterday > 0 
    ? ((model.SalesToday - salesYesterday) / salesYesterday) * 100 
    : 0;
```

### Add More Metrics
Add new properties to `DashboardViewModel.cs` and update the controller and view accordingly.

## Troubleshooting

### Connection Issues
- Verify MySQL is running
- Check connection string
- Ensure database exists
- Verify user permissions

### No Data Showing
- Check if orders exist in database
- Verify date ranges
- Check RDC filtering logic
- Ensure status values match ("DELIVERED", "COMPLETED", "PENDING")

### Authentication Errors
- Ensure user is logged in
- Check cookie configuration
- Verify claims are set correctly

## Next Steps

1. Integrate with your authentication system
2. Add more report types
3. Implement chart visualizations
4. Add export to Excel/PDF
5. Create scheduled reports
6. Add email notifications for low stock

## Support

For questions about this module, refer to:
- ASE Project Documentation
- System Requirements PDF
- Database Schema

---

**Author:** Developed for ASE Coursework Part 7  
**Date:** February 2026  
**Technology Stack:** ASP.NET Core 8.0, Entity Framework Core, MySQL, Razor Pages
