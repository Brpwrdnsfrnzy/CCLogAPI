using CCLogAPI.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CCLogAPI.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow the front-end origin(s)
var webAppBase = builder.Configuration["WebAppBaseUrl"]?.Trim();
// Build a conservative set of allowed origins including common variants
var origins = new List<string>();
if (!string.IsNullOrWhiteSpace(webAppBase))
{
    origins.Add(webAppBase);
    // Add https/http variants and with/without www
    try
    {
        var uri = new Uri(webAppBase);
        var host = uri.Host;
        var scheme = uri.Scheme;
        var otherScheme = scheme == "https" ? "http" : "https";
        origins.Add($"{otherScheme}://{host}");
        if (!host.StartsWith("www."))
        {
            origins.Add($"{scheme}://www.{host}");
            origins.Add($"{otherScheme}://www.{host}");
        }
        else
        {
            var withoutWww = host.Substring(4);
            origins.Add($"{scheme}://{withoutWww}");
            origins.Add($"{otherScheme}://{withoutWww}");
        }
    }
    catch { /* ignore parse errors */ }
}
// Common local dev origins
origins.Add("http://localhost:5000");
origins.Add("http://localhost:5001");
// Known front-end origins (production and staging)
origins.Add("http://cclogs.net");
origins.Add("http://www.cclogs.net");
origins.Add("http://staging.cclogs.net");
origins = origins.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        // Restrict CORS to the known front-end origins.
        policy.WithOrigins(origins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Note: do not call AllowCredentials() unless the frontend needs to send
        // cookies or other credentials. If you enable credentials you cannot use '*'.
    });
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register EF DbContext using DefaultConnection if available; otherwise rely on CCLogContext.OnConfiguring fallback
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(conn))
{
    builder.Services.AddDbContext<CCLogContext>(options => options.UseSqlServer(conn));
}
// Register read-only Sage DB context if configured
var sageConn = builder.Configuration.GetConnectionString("SageConnection");
if (!string.IsNullOrWhiteSpace(sageConn))
{
    builder.Services.AddDbContext<SageContext>(options => options.UseSqlServer(sageConn));
}

// Register ASP.NET Core Identity using the EF Core context with int keys
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Keep defaults but allow configuration via appsettings if needed
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<CCLogContext>()
    .AddDefaultTokenProviders();

// Email service configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Perform startup health checks (DB connectivity, etc.) and log clear diagnostics.
try
{
    using var startupScope = app.Services.CreateScope();
    var startupLogger = startupScope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("StartupChecks");
    var db = startupScope.ServiceProvider.GetService<CCLogContext>();
    if (db == null)
    {
        startupLogger?.LogWarning("CCLogContext is not registered. Skipping DB connectivity check.");
    }
    else
    {
        try
        {
            // Use CanConnect which performs a lightweight connectivity check.
            var canConnect = db.Database.CanConnect();
            if (canConnect)
            {
                startupLogger?.LogInformation("Database connectivity check passed.");
            }
            else
            {
                startupLogger?.LogError("Database connectivity check failed: unable to connect to the configured database. Verify connection string, network, and credentials.");
            }
        }
        catch (Exception ex)
        {
            startupLogger?.LogError(ex, "Exception while checking database connectivity. Connection string in use: {Connection}", builder.Configuration.GetConnectionString("DefaultConnection") ?? "(none)");
        }
    }
}
catch (Exception ex)
{
    // Ensure any unexpected error during startup checks is logged but does not crash the host.
    var tmpLogger = app.Services.GetService<ILoggerFactory>()?.CreateLogger("StartupChecks");
    tmpLogger?.LogError(ex, "Unexpected error during startup health checks.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Diagnostic middleware: log requests and provide an explicit OPTIONS fallback for CORS preflight.
// This helps verify whether requests reach the app and ensures an Access-Control-Allow-* header is returned
// even if something upstream short-circuits the pipeline.
// Global exception handler that ensures CORS headers are present on error responses
// so browser clients receive an Access-Control-Allow-Origin even when a 500 occurs.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var loggerFactory = app.Services.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("GlobalException");
        var originHeader = context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrWhiteSpace(originHeader) && origins.Contains(originHeader, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = originHeader;
            if (!context.Response.Headers.ContainsKey("Vary"))
                context.Response.Headers["Vary"] = "Origin";
        }

        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature?.Error != null)
        {
            logger?.LogError(exceptionFeature.Error, "Unhandled exception while processing request {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        // In Staging or Development, include exception details to aid debugging.
        var isDebugEnv = app.Environment.IsDevelopment() || app.Environment.IsStaging();
        if (isDebugEnv && exceptionFeature?.Error != null)
        {
            var msg = exceptionFeature.Error.Message?.Replace("\r\n", " ").Replace("\n", " ");
            var stack = exceptionFeature.Error.StackTrace?.Replace("\r\n", " ").Replace("\n", " ");
            var payload = new
            {
                error = "An unexpected error occurred.",
                details = System.Text.Encodings.Web.JavaScriptEncoder.Default.Encode(msg ?? string.Empty),
                stack = System.Text.Encodings.Web.JavaScriptEncoder.Default.Encode(stack ?? string.Empty)
            };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync(json);
        }
        else
        {
            await context.Response.WriteAsync("{\"error\":\"An unexpected error occurred.\"}");
        }
    });
});
app.Use(async (context, next) =>
{
    var loggerFactory = app.Services.GetService<ILoggerFactory>();
    var logger = loggerFactory?.CreateLogger("CorsDebug");
    var origin = context.Request.Headers["Origin"].ToString();
    logger?.LogInformation("Incoming request: {Method} {Path} Origin={Origin}", context.Request.Method, context.Request.Path, origin);

    if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        // For diagnostics, log OPTIONS preflight but do NOT short-circuit here.
        // Let the CORS middleware (registered later) handle preflight responses so
        // the configured policy is consistently applied by the framework.
        logger?.LogDebug("OPTIONS request received; forwarding to CORS middleware for handling.");
        await next();
        return;
    }

    await next();
});

// Lightweight preflight handler: respond to OPTIONS early with the proper CORS headers
// This ensures preflight requests get a deterministic response even if something
// upstream or later middleware would otherwise fail.
app.Use(async (context, next) =>
{
    try
    {
        if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var origin = context.Request.Headers["Origin"].ToString();
            var loggerFactory = app.Services.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("PreflightHandler");
            logger?.LogDebug("Preflight request for {Path} from {Origin}", context.Request.Path, origin);

            if (!string.IsNullOrWhiteSpace(origin) && origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
                // Echo requested headers if present, otherwise allow common headers
                var reqHeaders = context.Request.Headers["Access-Control-Request-Headers"].ToString();
                context.Response.Headers["Access-Control-Allow-Headers"] = string.IsNullOrWhiteSpace(reqHeaders) ? "*" : reqHeaders;
                context.Response.Headers["Vary"] = "Origin";
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }
        }
    }
    catch (Exception ex)
    {
        var loggerFactory = app.Services.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("PreflightHandler");
        logger?.LogError(ex, "Error handling preflight request for {Path}", context.Request.Path);
        // Ensure we still return a response (with no CORS header) so callers see an error rather than nothing
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Preflight handler error");
        return;
    }

    await next();
});
app.UseRouting();
app.UseCors("DefaultCorsPolicy");
// Ensure CORS header is present on all responses when an Origin header was sent.
// This is a defensive fallback for environments where the CORS middleware
// may be bypassed (proxies/reverse-proxies or middleware ordering differences).
app.Use(async (context, next) =>
{
    await next();

    // If the request included an Origin header and the response doesn't already
    // have an Access-Control-Allow-Origin header, add one matching the policy.
    if (context.Request.Headers.ContainsKey("Origin") && !context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
    {
        var origin = context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrWhiteSpace(origin) && origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            if (!context.Response.Headers.ContainsKey("Vary"))
                context.Response.Headers["Vary"] = "Origin";
        }
    }
});
app.UseAuthorization();

app.MapControllers();

app.Run();
