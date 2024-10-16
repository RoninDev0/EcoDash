using Microsoft.AspNetCore.Authentication.Cookies;
using EcoDash.Data;
using EcoDash.Services;
using dotenv.net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Load environment variables from .env file
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services, builder);

// Build the application
var app = builder.Build();

// Configure middleware
ConfigureMiddleware(app);

// Run the application
app.Run();

// Helper methods
void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
{
    // Read environment variables directly
    var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
    var databaseName = Environment.GetEnvironmentVariable("DatabaseName");

    // Validate that the environment variables are not empty
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
    {
        throw new Exception("ConnectionString or DatabaseName environment variables are not set.");
    }

    // MongoDB context registration using environment variables
    services.AddSingleton<MongoDbContext>(sp =>
    {
        return new MongoDbContext(connectionString, databaseName);
    });

    // Register controllers with views
    services.AddControllersWithViews();

    // Register RouteService using HttpClient
    services.AddHttpClient<RouteService>();

    // Enable CORS if needed
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    // Authentication and Authorization
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Login/Index";
        });

    services.AddAuthorization();

    // Add HSTS settings for production
    if (!builder.Environment.IsDevelopment())
    {
        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365); // 1 year HSTS policy
            options.IncludeSubDomains = true; // Apply HSTS to subdomains
            options.Preload = true; // Preload HSTS across all domains
        });
    }
}

void ConfigureMiddleware(WebApplication app)
{
    // Error handling for production
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error?error=true");
        app.UseHsts();  // Enforce HSTS for production
    }

    // Redirect HTTP requests to HTTPS
    app.UseHttpsRedirection();

    // Middleware pipeline
    app.UseStaticFiles();
    app.UseRouting();

    // Enable authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Enable CORS
    app.UseCors("AllowAll");

    // Route configuration
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}
