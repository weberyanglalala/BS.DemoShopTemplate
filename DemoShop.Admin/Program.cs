using DemoShop.Admin.Configurations;
using DemoShop.Admin.Models.Settings;
using DemoShop.ApplicationCore.Interfaces;
using DemoShop.ApplicationCore.Interfaces.TodoService;
using DemoShop.ApplicationCore.Services;
using DemoShop.Infrastructure;
using DemoShop.Infrastructure.Data;

namespace DemoShop.Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            Dependencies.ConfigureServices(builder.Configuration, builder.Services);
            builder.Services
                .AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddWebSettings(builder.Configuration);
            
            builder.Services
                .AddApplicationCoreServices()
                .AddAuthServices()
                .AddSwaggerService()
                .AddWebServices(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}