using System.Text;
using CCLogAPI.Models;
using CCLogAPI.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Ensure logs directory exists
var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsDirectory))
{
    Directory.CreateDirectory(logsDirectory);
}

Serilog.Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/api.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Serilog.Log.Information("Starting CCLog API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    const string CorsPolicyName = "AllowFrontend";

    // Add services to the container.

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // Register EF Core DbContext with connection string from configuration
    builder.Services.AddDbContext<CCLogContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Register Sage200 DbContext (read-only, for SS_Bris_cust view)
    builder.Services.AddDbContext<SageContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SageConnection")));

    // Identity - use AddIdentityCore for API (not AddIdentity which is for cookie auth)
    builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
        .AddRoles<IdentityRole<int>>()
        .AddEntityFrameworkStores<CCLogContext>()
        .AddSignInManager<SignInManager<User>>()
        .AddDefaultTokenProviders();

    // JWT configuration - set Jwt:Key in secrets or appsettings
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisWithAStrongSecretInUserSecrets";
    var key = Encoding.UTF8.GetBytes(jwtKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        // Override Identity's default "require authenticated user" policy
        options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true) // Allow all by default
            .Build();
        options.FallbackPolicy = null;
    });

    // Register AutoMapper (requires AutoMapper.Extensions.Microsoft.DependencyInjection package)
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // Email and scheduled reporting
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<DailyReportJob>();
    builder.Services.AddScoped<ReminderCheckJob>();
    builder.Services.AddScoped<EscalationCheckJob>();
    builder.Services.AddHostedService<DailyReportScheduler>();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // CORS: allow the Blazor frontend to call the API
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            policy.WithOrigins(
                "http://www.cclogs.net",
                "http://cclogs.net",
                "https://www.cclogs.net",
                "https://cclogs.net",
                "http://www.cclogsapi.net",
                "http://cclogsapi.net",
                "https://www.cclogsapi.net",
                "https://cclogsapi.net",
                "https://localhost:7260",  // Local Blazor app
                "http://localhost:7260",
                "https://localhost:5000",   // Common dev ports
                "http://localhost:5000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Seed admin user
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        await SeedAdminUser(userManager);
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Add exception handler middleware to catch unhandled exceptions
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (error != null)
            {
                Serilog.Log.Error(error.Error, "Unhandled exception in API request");
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "An error occurred processing your request.",
                    details = error.Error.Message
                }));
            }
        });
    });

    // CORS must come before Authentication/Authorization
    app.UseCors(CorsPolicyName);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Serilog.Log.Information("CCLog API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Serilog.Log.Fatal(ex, "CCLog API failed to start");
    throw;
}
finally
{
    Serilog.Log.CloseAndFlush();
}

async Task SeedAdminUser(UserManager<User> userManager)
{
    try
    {
        const string adminUsername = "rlockwood";
        const string adminEmail = "rlockwood@brisant-secure.com";

        var existingUser = await userManager.FindByNameAsync(adminUsername);
        if (existingUser == null)
        {
            var adminUser = new User
            {
                UserName = adminUsername,
                Email = adminEmail,
                Name = "Administrator",
                Role = 1,
                Team = 1,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "ziggy321");
            if (result.Succeeded)
            {
                Serilog.Log.Information("✅ Admin user seeded successfully.");
            }
            else
            {
                Serilog.Log.Error("❌ Failed to seed admin user:");
                foreach (var error in result.Errors)
                {
                    Serilog.Log.Error($"  - {error.Description}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Serilog.Log.Error(ex, "Error seeding admin user");
    }
}
