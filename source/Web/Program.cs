using Demo.Composition;
using Demo.Infrastructure.Catalog;
using Demo.Infrastructure.Identity;
using Demo.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Register the AutoValidateAntiforgeryTokenAttribute as a global filter
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", policy =>
    {
        policy.PermitLimit = 10;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        policy.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddComposition(builder.Configuration);

builder.Services.AddCookiePolicy(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    // Handle the authentication ticket creation
    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var provisioner = context.HttpContext.RequestServices
                .GetRequiredService<Web.Auth.GoogleUserProvisioningService>();
            await provisioner.ProvisionAsync(context);
        }
    };

    // Fix SameSite cookie issues for OAuth state
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.CorrelationCookie.HttpOnly = true;
});

builder.Services.AddScoped<Web.Auth.GoogleUserProvisioningService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Web.Policies.ActiveSellerHandler>();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("ActiveSeller", policy =>
        policy.AddRequirements(new Web.Policies.ActiveSellerRequirement()));
});

var app = builder.Build();

// Apply pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<DemoIdentityDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        logger.LogError(ex, "Database migration failed on startup");
    }
}

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        await DataSeeder.SeedRolesAsync(roleManager, logger);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        logger.LogError(ex, "Role seeding failed on startup");
    }
}

// Seed fee categories on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<DemoCatalogDbContext>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        await DataSeeder.SeedFeeCategoriesAsync(dbFactory, logger);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        logger.LogError(ex, "Fee category seeding failed on startup");
    }
}

// Seed product categories on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<DemoCatalogDbContext>>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        await DataSeeder.SeedProductCategoriesAsync(dbFactory, logger);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        logger.LogError(ex, "Category seeding failed on startup");
    }
}

// Configure cookie policy for SameSite issues
app.UseCookiePolicy();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Content Security Policy
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://accounts.google.com https://apis.google.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "connect-src 'self' https://accounts.google.com https://oauth2.googleapis.com; " +
        "frame-src 'self' https://accounts.google.com;");

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRateLimiter();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Anonymous}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

app.Run();
