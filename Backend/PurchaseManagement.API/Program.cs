using Microsoft.EntityFrameworkCore;
using PurchaseManagement.API.Data;
using PurchaseManagement.API.Repositories;
using PurchaseManagement.API.Services;
using Scalar.AspNetCore;

// ── Application Builder ────────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

// ── Database Configuration ─────────────────────────────────────────────────────
// Connection string must be set in appsettings.json under "ConnectionStrings:DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)));

// ── Repository Registrations (Dependency Injection) ───────────────────────────
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IPurchaseBillRepository, PurchaseBillRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// ── Service Registrations ──────────────────────────────────────────────────────
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPurchaseBillService, PurchaseBillService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// ── MVC Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON properties to match Angular conventions
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ── Swagger / OpenAPI ──────────────────────────────────────────────────────────
// Uses the built-in .NET 9 OpenAPI support (no Swashbuckle needed)
builder.Services.AddOpenApi();

// ── CORS Configuration ─────────────────────────────────────────────────────────
// Allow Angular frontend (http://localhost:4200) to access this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Build Application ──────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate and seed database on startup ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Apply any pending EF Core migrations automatically
    db.Database.Migrate();
}

// ── Middleware Pipeline ────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // Native .NET 9 OpenAPI document served at /openapi/v1.json
    app.MapOpenApi();
    // Scalar API explorer UI served at /scalar/v1
    app.MapScalarApiReference();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();

// No authentication middleware — skipped as per Task 1
app.MapControllers();

app.Run();
