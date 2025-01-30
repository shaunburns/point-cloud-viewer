using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PointCloudViewer.Server;

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

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT authentication
var key = System.Text.Encoding.ASCII.GetBytes("YourSecretKeyHere"); // Use a secure key in production
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = "YourIssuer",
        ValidAudience = "YourAudience",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    SeedAdminUser(app);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

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
