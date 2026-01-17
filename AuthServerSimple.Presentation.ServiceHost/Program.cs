using AuthServerSimple.Application;
using AuthServerSimple.Application.Interfaces;
using AuthServerSimple.Infrastructure.Identity;
using AuthServerSimple.Infrastructure.Identity.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);

    // Overriding some system lib logging
    config.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);
    config.MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning);
});

// Application Layer: Options, Services & Validation
builder.AddApplicationLayerDependencies();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Auth
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();

//Controllers & Open API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply Db migrations
await app.Services.ApplyMigrationsAsync();

// Seed roles and (optionally) users 
await DbInitializer.SeedRolesAsync(app.Services);
await DbInitializer.SeedUsersAsync(app.Services);

//Serilog
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Auth
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();