using Microsoft.EntityFrameworkCore;
using ISDN.Models;
using ISDN.Interfaces;
using ISDN.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller and Views 
builder.Services.AddControllersWithViews();

// 2. Database  connection (MySQL)
var connectionString = "server=localhost;port=3306;database=isdn_distribution_db;user=root;password=1998@Admin";
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(connectionString));

// 3. Repository   (Dependency Injection)
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

var app = builder.Build();

// Middleware  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//Payment Controller  Index  Open 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Payment}/{action=Index}/{id?}");

app.Run();