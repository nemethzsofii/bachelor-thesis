﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebApplication1.Data;
using WebApplication1.Model;
using QuestPDF;
using QuestPDF.Infrastructure;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

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
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; 
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

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            //  Register controllers and Razor Pages
            app.MapControllers();
            app.MapRazorPages();

            // Handle "/" route
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