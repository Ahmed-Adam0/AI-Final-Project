using Graduation_domain.Entities;
using Graduation_infrastructure.AppDbContext;
using Graduation_infrastructure.ProgramService.ServicesAPI;
using Graduation_infrastructure.SignalR;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

namespace Graduation_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddInfrastructureAPI(builder.Configuration);

            builder.Services.AddOpenApi();
            //builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

           

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var context = services.GetRequiredService<ApplicationDbContext>();

                await ApplicationDbSeeder.SeedAsync(services, userManager, roleManager, context);
            }

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //}
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<InternalNotificationHub>("/hubs/notifications");

            app.MapControllers();

            app.Run();
        }
    }
}