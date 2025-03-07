using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using PointCloudViewer.Server;
using PointCloudViewer.Server.Services;
using PointCloudViewer.Server.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("DefaultConnection") ?? "AppDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=AppDb.db"));
}

ConfigureServices(builder.Services);

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureJwtAuth(builder.Configuration); 

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Ensure the database is created and opened
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    app.MapOpenApi();
    SeedAdminUser(app);

    // This should be disabled in any production workloads as it will fail GDPR compliance
    // Enable if required to debug locally:
    // IdentityModelEventSource.ShowPII = true;
    // IdentityModelEventSource.LogCompleteSecurityArtifact = true;
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

static void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<ITokenService, TokenService>();
}

static void SeedAdminUser(IHost app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;

    var adminRole = "Admin";
    var adminEmail = "admin@example.com";
    var adminUsername = "admin";
    var adminPassword = "admin";

    // Temporarily disable password requirements
    var originalPasswordOptions = identityOptions.Password;
    identityOptions.Password.RequireDigit = false;
    identityOptions.Password.RequiredLength = 1;
    identityOptions.Password.RequireNonAlphanumeric = false;
    identityOptions.Password.RequireUppercase = false;
    identityOptions.Password.RequireLowercase = false;

    try
    {
        // Ensure the admin role exists
        if (!roleManager.RoleExistsAsync(adminRole).Result)
        {
            roleManager.CreateAsync(new IdentityRole(adminRole)).Wait();
        }

        // Ensure the admin user exists
        var adminUser = userManager.FindByEmailAsync(adminEmail).Result;
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminUsername, Email = adminEmail };
            var result = userManager.CreateAsync(adminUser, adminPassword).Result;
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, adminRole).Wait();
            }
        }
    }
    finally
    {
        // Restore original password options
        identityOptions.Password = originalPasswordOptions;
    }
}

static class JwtTokenConfigurationExtensions
{
    public static void ConfigureJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        string? jwtKey = config["Jwt:Key"];
        if (jwtKey == null)
        {
            // TODO: Log Error of bad JWT configuration
            return;
        }

        // Configure JWT authentication
        var key = System.Text.Encoding.UTF8.GetBytes(jwtKey);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };
        });
    }
}

public partial class Program { }
