using Graduation_infrastructure.ProgramService.ServicesMVC;
using Microsoft.Extensions.FileProviders;

namespace Graduation_MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================
            // SERVICES
            // =====================
            builder.Services.AddControllersWithViews();

            // Infrastructure (MVC services)
            builder.Services.AddInfrastructureMVC(builder.Configuration);

            var app = builder.Build();

            // =====================
            // ERROR HANDLING
            // =====================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // =====================
            // PIPELINE
            // =====================
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Serve product images written by the API under Graduation-API/wwwroot (e.g. /images/products/...)
            var apiWwwRootSetting = builder.Configuration["ApiSettings:WwwRootPath"];
            var apiWwwRoot = string.IsNullOrWhiteSpace(apiWwwRootSetting)
                ? Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "Graduation-API", "wwwroot"))
                : Path.GetFullPath(Path.IsPathRooted(apiWwwRootSetting)
                    ? apiWwwRootSetting
                    : Path.Combine(app.Environment.ContentRootPath, apiWwwRootSetting));

            if (Directory.Exists(apiWwwRoot))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(apiWwwRoot),
                    RequestPath = ""
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            // =====================
            // ROUTING (IMPORTANT)
            // =====================

            // 🔥 Areas Route (Admin, etc.)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Default Route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}