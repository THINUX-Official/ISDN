using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ISDN.Data;
using ISDN.Models;
using ISDN.Services;
using ISDN.Repositories;
using ISDN.Middleware;
using ISDN_Distribution.Repositories;
using ISDN_Distribution.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Settings from appsettings.json using Options pattern
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Also register as Singleton for direct injection (used in Program.cs)
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() 
    ?? throw new InvalidOperationException("JWT settings not configured");

// Validate JWT Secret is not empty
if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException($"JWT Secret is empty or null! Check appsettings.json. Secret length: {jwtSettings.Secret?.Length ?? 0}");
}

Console.WriteLine($"JWT Settings Loaded: Secret length = {jwtSettings.Secret.Length}, Issuer = {jwtSettings.Issuer}");

builder.Services.AddSingleton(jwtSettings);

// Configure Entity Framework Core with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<IsdnDbContext>(options =>
options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Read JWT from cookie for MVC views
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["AuthToken"];
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization Policies for Role-Based Access Control
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("HeadOfficeOnly", policy => policy.RequireRole("ADMIN", "HEAD_OFFICE"));
    options.AddPolicy("RdcStaffOnly", policy => policy.RequireRole("ADMIN", "HEAD_OFFICE", "RDC_STAFF"));
    options.AddPolicy("LogisticsOnly", policy => policy.RequireRole("ADMIN", "HEAD_OFFICE", "LOGISTICS"));
    options.AddPolicy("DriverOnly", policy => policy.RequireRole("ADMIN", "LOGISTICS", "DRIVER"));
    options.AddPolicy("FinanceOnly", policy => policy.RequireRole("ADMIN", "HEAD_OFFICE", "FINANCE"));
    options.AddPolicy("SalesRepOnly", policy => policy.RequireRole("ADMIN", "HEAD_OFFICE", "SALES_REP"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("CUSTOMER"));
    options.AddPolicy("InternalStaff", policy => policy.RequireRole("ADMIN", "HEAD_OFFICE", "RDC_STAFF", "LOGISTICS", "DRIVER", "FINANCE", "SALES_REP"));
});

// Register Services and Repositories with Dependency Injection
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Program.cs එකේ මේ පේළිය එකතු කරන්න
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Add HttpContextAccessor for accessing HTTP context in services
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IRdcOrderRepository, RdcOrderRepository>();

// Add Session support for temporary data storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed database with roles and default users (SYNCHRONOUS - must complete before app starts)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting database initialization...");
        
        var context = services.GetRequiredService<IsdnDbContext>();
        
        // Test database connection first
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to MySQL database. Please check your connection string in appsettings.json");
            throw new Exception("Database connection failed. Check appsettings.json");
        }
        
        logger.LogInformation("Database connection successful.");
        
        // Ensure database and all tables are created
        logger.LogInformation("Creating database schema (tables)...");
        var created = await context.Database.EnsureCreatedAsync();
        if (created)
        {
            logger.LogInformation("Database tables created successfully.");
        }
        else
        {
            logger.LogInformation("Database tables already exist.");
        }
        
        // Seed data
        logger.LogInformation("Starting data seeding...");
        var auditService = services.GetRequiredService<IAuditLogService>();
        await IsdnDbInitializer.SeedDataAsync(context, auditService);
        
        logger.LogInformation("Database seeding completed successfully.");
        logger.LogInformation("All 27 users are ready to use.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Critical: Database initialization failed");
        logger.LogError($"Error details: {ex.Message}");
        if (ex.InnerException != null)
        {
            logger.LogError($"Inner exception: {ex.InnerException.Message}");
        }
        logger.LogError("Application will exit. Please fix database issues and restart.");
        throw; // Stop application startup if database fails
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // In Development: Show detailed error pages with full stack traces
    app.UseDeveloperExceptionPage();
}
else
{
    // In Production: Use generic error handler
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Use custom JWT middleware
app.UseMiddleware<JwtMiddleware>();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Use session
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add a health check endpoint
app.MapGet("/health", async (IsdnDbContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            return Results.Ok(new { status = "healthy", database = "connected", timestamp = DateTime.UtcNow });
        }
        return Results.Problem("Database connection failed", statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Health check failed: {ex.Message}", statusCode: 503);
    }
});

app.Run();
