using Microsoft.AspNetCore.Authentication.Cookies;
using EcoDash.Data;
using EcoDash.Services;

dotenv.net.DotEnv.Load(options: new dotenv.net.DotEnvOptions(probeForEnv: true));

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure middleware
ConfigureMiddleware(app);

app.Run();

// Helper methods
void ConfigureServices(IServiceCollection services)
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

    // add routes service
    builder.Services.AddHttpClient<RouteService>();

    //Authentication
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
    });

    services.AddAuthorization();
}

void ConfigureMiddleware(WebApplication app)
{
    // Error handling
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error?error=true");
        app.UseHsts();
    }

    // Middleware pipeline
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    // Route configuration
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}
