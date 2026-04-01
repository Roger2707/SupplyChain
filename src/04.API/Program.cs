using ECommerce.Infrastructure.Data;
using Identity.Infrastructure.Data;
using Inventory.Infrastructure.Seed;
using Inventory.Infrastructure.Services;
using SupplyChain.WebApi.Middleware;
using SupplyChain.WebApi.Middlewares;
using SupplyChain.WebApi.Policies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Seed;
using Inventory.Infrastructure.Data;
using SharedKernel.Services;
using SharedKernel.Interfaces;
using SupplyChain.WebApi.Services;
using ECommerce.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);    

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input JWT Token here: "
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Database Configuration (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(connectionString, b =>
        b.MigrationsAssembly("Identity.Infrastructure")
         .MigrationsHistoryTable("__EFMigrationsHistory_Identity")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, b =>
        b.MigrationsAssembly("Inventory.Infrastructure")
         .MigrationsHistoryTable("__EFMigrationsHistory_Inventory")));

builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(connectionString, b =>
        b.MigrationsAssembly("ECommerce.Infrastructure")
         .MigrationsHistoryTable("__EFMigrationsHistory_ECommerce")));


builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddInventoryServices();
builder.Services.AddIdentityServices();
builder.Services.AddEcommerceServices();

#region Redis Cache

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

#endregion

builder.Services.AddScoped<SeedIdentityService>();
builder.Services.AddScoped<SeederService>();

builder.Services.AddScoped<Identity.Application.Interfaces.IAuthenticationService, Identity.Application.Services.AuthenticationService>();

#region Authentication Configuration

// Configure Authentication (Custom JWT Handler)
builder.Services
    .AddAuthentication("JwtAuthentication")
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>("JwtAuthentication", options => { });

#endregion

#region Authorization Configuration

// Configure Authorization with Permission policies
builder.Services.AddAuthorization(options =>
{
    // 1. DefaultPolicy cho [Authorize] no parameters
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // 2. Custom policies cho [Authorize(Policy = "...")]
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("Super_Admin"));

    options.AddPolicy("RegionalOrAbove", policy =>
    policy.RequireRole("Super_Admin", "Regional_Manager"));

    options.AddPolicy("ManagerOrAbove", policy =>
        policy.RequireRole("Super_Admin", "Regional_Manager", "Warehouse_Manager"));

    // WAREHOUSE PERMISSION POLICIES
    options.AddPolicy("CaUpdateWarehouse", policy =>
        policy.Requirements.Add(new WarehousePermissionRequirement("Warehouse", "Update")));

    options.AddPolicy("CanDeleteWarehouse", policy =>
        policy.Requirements.Add(new WarehousePermissionRequirement("Warehouse", "Delete")));

    options.AddPolicy("CanViewWarehouse", policy =>
        policy.Requirements.Add(new WarehousePermissionRequirement("Warehouse", "View")));
});

#endregion

// Register Permission Authorization Handler
builder.Services.AddScoped<IAuthorizationHandler, WarehousePermissionHandler>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// Custom Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventorySystem API v1");
    c.RoutePrefix = "swagger"; // swagger UI at /swagger
});

app.UseCors("AllowAll");

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var seederIdentity = services.GetRequiredService<SeedIdentityService>();
    var seederInventory = services.GetRequiredService<SeederService>();

    await seederIdentity.SeedDataAsync();
    await seederInventory.SeedDataAsync();
}

app.Run();
