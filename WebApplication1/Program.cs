using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebApplication1.Data;
using WebApplication1.Model;


namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Get MySQL connection string from configuration
            string connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            // Add Authentication & Authorization services
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization();

            //  Add MVC & Razor Pages
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            builder.Services.AddRazorPages();

            // Enable Swagger API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //  CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add Session Services
            builder.Services.AddDistributedMemoryCache(); // Required for storing sessions
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust session timeout as needed
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Required for GDPR compliance
            });

            // Add HttpContextAccessor for accessing session in controllers
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Enable CORS
            app.UseCors("AllowAll");

            // Configure error handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Enable Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // Use HTTPS and static files
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Ensure routing is registered before authentication
            app.UseRouting();

            app.UseSession();

            //  Ensure authentication & authorization middleware are in the correct order
            app.UseAuthentication();
            app.UseAuthorization();

            //  Register controllers and Razor Pages
            app.MapControllers();
            app.MapRazorPages();

            // Handle "/" route properly to avoid infinite redirects
            app.MapGet("/", (HttpContext httpContext) =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    return Results.Redirect("/Index"); // If authenticated, go to main page
                }
                return Results.Redirect("/Account/Login"); // If not, go to login
            });

            app.Run();
        }
    }
}